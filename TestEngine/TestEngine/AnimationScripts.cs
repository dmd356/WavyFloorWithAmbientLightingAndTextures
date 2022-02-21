using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
namespace TestEngine
{
    /// <summary>
    /// Animation Key Frame
    /// </summary>
    public struct AnimKeyFrame
    {
        /// <summary>
        /// The moment in time where this is;
        /// </summary>
        public float KeyFrameCursor { get; set; }

        /// <summary>
        /// Get whatever vertex data is here
        /// <para>X,Y,Z</para>
        /// </summary>
        public Vector3[] PositionToMoveTo { get; set; }


    }

    /// <summary>
    ///
    /// </summary>
    public class AnimBreather {
        /// <summary>
        /// Yea, these be the parameters for the Animations that breath life into the game.
        /// </summary>
        public Dictionary<string, Animation> Animations { get; set; }
        public List<float> FloatIds { get; set; }
    }



    public class Animation
    {


        public string AnimationName { get; set; }

        public bool IsLoopingAnim { get; set; }

        /// <summary>
        /// Length measured in seconds via float
        /// </summary>
        public float LengthInSeconds { get; set; }

        /// <summary>
        /// The Key frames to render.
        /// The Keyframe Cursor Determines the ID;
        /// </summary>
        public Dictionary<float, AnimKeyFrame> AnimKeyFrames { get; set; }



        /// <summary>
        /// Ussually 60
        /// </summary>
        public float FramesPerSecond { get; set; }

        public float TotalFrames { get => FramesPerSecond * LengthInSeconds; }
    }

    public enum GridControllerEnums
    {
        WaveyMode,
    }
    public static class GameEngine
    {
        public static void RunApplication()
        {
            using (var game = new GlassRenderer(1600, 1000, "Test_Engine"))
            {
                game.StartRenderer();
            }
        }
        public static void AnimHandler_UpdateFrame()
        {
            GridAnimationHandler.UpdateFrame();
        }

        public static Animation GridAnimationGet()
        {
            return GridAnimationHandler.GridViewAnim;
        }
    }

    public static class AnimationFactory
    {
        public static int CellSize = 5;

        public static int CellSizeX = 5;
        public static int CellSizeZ = 5;

        #region Try these out
        ////Flappy Grid! Like a bunch of trap doors.
        //gridVertices.AddRange(new List<Vector3>
        //                {
        //                    new Vector3(x,       0f, z     ),//0
        //                    new Vector3(x + 1f,  0f , z     ),//1
        //                    new Vector3(x, modY + 0f, z + 1f),//2
        //                    new Vector3(x, modY + 0f, z + 1f),//2
        //                    new Vector3(x + 1f, modY +0f, z + 1f),//3
        //                    new Vector3(x + 1f,  0f, z     ),//1
        //                });


        /// <summary>
        /// Panels flap ip (y) across the x axis
        /// </summary>
        public static Animation GridAnimationGetFlappyTraps()
        {
            Animation gridAnimation = new Animation()
            {
                AnimationName = GridControllerEnums.WaveyMode.ToString(),
                LengthInSeconds = 1.3f,//1.3 seconds
                FramesPerSecond = 60.0f,
                IsLoopingAnim = true,
            };
            var keys = new Dictionary<float, AnimKeyFrame>();

            float currX = 0f;
            float YCapHeight = 1f;

            //My example this will be 1/7.8 = 0.1282051282051282
            float moveUpDownYPerFrame = YCapHeight / (gridAnimation.TotalFrames / CellSizeX);

            var framesPerX = gridAnimation.TotalFrames / CellSize;//How much this x can move UP before moving on.

            //When currX > 0, we want to lower the last X's 'Y' 
            //so it goes down WHILE the new one goes up
            var saveLastYHeightForMinus = 0f;
            var saveCurrYHeightForAddition = 0f;

            for (float currFrame = 0; currFrame < gridAnimation.TotalFrames; currFrame += 1f)
            {
                var gridVertices = new List<Vector3>();

                for (float x = 0f; x < CellSizeX; x += 1f)
                {
                    bool onCurrX = x == currX;
                    bool onLastXBeforeCurr = currX - 1 >= 0 && x == currX - 1;
                    float modY = 0f;

                    if (onCurrX)
                    {
                        modY += moveUpDownYPerFrame + saveCurrYHeightForAddition;
                        saveCurrYHeightForAddition = modY;
                    }
                    else if (onLastXBeforeCurr)
                    {
                        modY += saveLastYHeightForMinus - moveUpDownYPerFrame;
                        saveLastYHeightForMinus = modY;
                    }

                    for (float z = 0f; z < CellSizeZ; z += 1f)
                    {
                        gridVertices.AddRange(new List<Vector3>
                        {
                            new Vector3(x,       0f, z     ),//0
                            new Vector3(x + 1f,  modY + 0f , z     ),//1
                            new Vector3(x,       0f, z + 1f),//2
                            new Vector3(x,       0f, z + 1f),//2
                            new Vector3(x + 1f,  modY +0f, z + 1f),//3
                            new Vector3(x + 1f,  modY + 0f, z     ),//1
                        });
                    }
                }
                keys.Add(currFrame, new AnimKeyFrame
                {
                    KeyFrameCursor = currFrame,
                    PositionToMoveTo = gridVertices.ToArray()
                });

                if (currFrame >= framesPerX)
                {
                    framesPerX = currFrame + (gridAnimation.TotalFrames / CellSizeX);
                    currX++;
                    saveLastYHeightForMinus = saveCurrYHeightForAddition;
                    saveCurrYHeightForAddition = 0f;
                }
            }
            gridAnimation.AnimKeyFrames = keys;
            return gridAnimation;
        }

        #endregion


        /// <summary>
        /// Grid and Collor. Sets mesh and render of Gridview
        /// </summary>
        public static Animation GridAnimationGetWavyConnected()
        {
            Animation gridAnimation = new Animation()
            {
                AnimationName = GridControllerEnums.WaveyMode.ToString(),
                LengthInSeconds = (float) CellSizeZ + 0.3f,//1.3 seconds
                FramesPerSecond = 60.0f,
                IsLoopingAnim = true,
            };
            var keys = new Dictionary<float, AnimKeyFrame>();

            float currX = 0f;
            float YCapHeight = 1f;

            //My example this will be 1/7.8 = 0.1282051282051282
            float moveUpDownYPerFrame = YCapHeight / (gridAnimation.TotalFrames / CellSizeX);

            var framesPerX = gridAnimation.TotalFrames / CellSizeX;//How much this x can move UP before moving on.

            //When currX > 0, we want to lower the last X's 'Y' 
            //so it goes down WHILE the new one goes up
            var saveLastYHeightForMinus = 0f;
            var saveCurrYHeightForAddition = 0f;
            var saveCurrLastYHeightForMinus = 0f;
            for (float currFrame = 0; currFrame < gridAnimation.TotalFrames; currFrame += 1f)
            {
                var gridVertices = new List<Vector3>();

                for (float x = 0f; x < CellSizeX; x += 1f)
                {
                    bool onCurrX = x == currX;
                    bool onAfterCurrX = x - 1 == currX;
                    bool onLastXBeforeCurr = currX - 1 >= 0 && x == currX - 1;
                    float modY = 0f;
                    float modYLastCurrent = 0f;

                    float modYAfter = 0f;
                    if (onCurrX)
                    {
                        modY += moveUpDownYPerFrame + saveCurrYHeightForAddition;
                        saveCurrYHeightForAddition = modY;
                        modYLastCurrent = currX!=0 ? saveCurrLastYHeightForMinus - moveUpDownYPerFrame : 0;
                        saveCurrLastYHeightForMinus = currX != 0 ? modYLastCurrent : saveCurrLastYHeightForMinus;
                    }
                    else if (onLastXBeforeCurr)
                    {
                        modY += saveLastYHeightForMinus - moveUpDownYPerFrame;
                        saveLastYHeightForMinus = modY;
                    }
                    else if (onAfterCurrX)
                    {
                        modYAfter += saveCurrYHeightForAddition;
                    }

                    for (float z = 0f; z < CellSizeZ; z += 1f)
                    {
                        gridVertices.AddRange(new List<Vector3>
                        {
                            new Vector3(x,      modYLastCurrent + modYAfter + 0f, z     ),//0
                            new Vector3(x + 1f,  modY + 0f , z     ),//1
                            new Vector3(x,       modYLastCurrent + modYAfter + 0f, z + 1f),//2
                            new Vector3(x,       modYLastCurrent + modYAfter + 0f, z + 1f),//2
                            new Vector3(x + 1f,  modY +0f, z + 1f),//3
                            new Vector3(x + 1f,  modY + 0f, z     ),//1
                        });
                    }
                }
                keys.Add(currFrame, new AnimKeyFrame
                {
                    KeyFrameCursor = currFrame,
                    PositionToMoveTo = gridVertices.ToArray()
                });

                if (currFrame >= framesPerX)
                {
                    framesPerX = currFrame + (gridAnimation.TotalFrames / CellSizeX);
                    currX++;
                    saveCurrLastYHeightForMinus = saveLastYHeightForMinus = saveCurrYHeightForAddition;
                    saveCurrYHeightForAddition = 0f;
                }
            }
            gridAnimation.AnimKeyFrames = keys;
            return gridAnimation;
        }

        /// <summary>
        /// Grid and Collor. Sets mesh and render of Gridview
        /// </summary>
        public static Animation GridAnimationGet()
        {
            Animation gridAnimation = new Animation()
            {
                AnimationName = GridControllerEnums.WaveyMode.ToString(),
                LengthInSeconds = 1.3f,//1.3 seconds
                FramesPerSecond = 60.0f,
                IsLoopingAnim = true,
            };
            var keys = new Dictionary<float, AnimKeyFrame>();

            float currX = 0f;
            float YCapHeight = 1f;

            //My example this will be 1/7.8 = 0.1282051282051282
            float moveUpDownYPerFrame = YCapHeight / (gridAnimation.TotalFrames / CellSize);

            var framesPerX = gridAnimation.TotalFrames / CellSizeX;//How much this x can move UP before moving on.

            //When currX > 0, we want to lower the last X's 'Y' 
            //so it goes down WHILE the new one goes up
            var saveLastYHeightForMinus = 0f;
            var saveCurrYHeightForAddition = 0f;

            for (float currFrame = 0; currFrame < gridAnimation.TotalFrames; currFrame += 1f)
            {
                var gridVertices = new List<Vector3>();

                for (float x = 0f; x < CellSizeX; x += 1f)
                {
                    bool onCurrX = x == currX;
                    bool onLastXBeforeCurr = currX - 1 >= 0 && x == currX - 1;
                    float modY = 0f;

                    if (onCurrX)
                    {
                        modY += moveUpDownYPerFrame + saveCurrYHeightForAddition;
                        saveCurrYHeightForAddition = modY;
                    }
                    else if (onLastXBeforeCurr)
                    {
                        modY += saveLastYHeightForMinus - moveUpDownYPerFrame;
                        saveLastYHeightForMinus = modY;
                    }

                    for (float z = 0f; z < CellSizeZ; z += 1f)
                    {
                        gridVertices.AddRange(new List<Vector3>
                        {
                            new Vector3(x,       0f, z     ),//0
                            new Vector3(x + 1f,  modY + 0f , z     ),//1
                            new Vector3(x,       0f, z + 1f),//2
                            new Vector3(x,       0f, z + 1f),//2
                            new Vector3(x + 1f,  modY +0f, z + 1f),//3
                            new Vector3(x + 1f,  modY + 0f, z     ),//1
                        });
                    }
                }
                keys.Add(currFrame, new AnimKeyFrame
                {
                    KeyFrameCursor = currFrame,
                    PositionToMoveTo = gridVertices.ToArray()
                });
                
                if(currFrame >= framesPerX)
                {
                    framesPerX = currFrame + (gridAnimation.TotalFrames / CellSizeX);
                    currX++;
                    saveLastYHeightForMinus = saveCurrYHeightForAddition;
                    saveCurrYHeightForAddition = 0f;
                }
            }
            gridAnimation.AnimKeyFrames = keys;
            return gridAnimation;
        }              
    }


    public static class GridAnimationHandler
    {

        public static Animation GridViewAnim { get; set; }
        static float CurrentFrame { get; set; }
        static float TotalFrames { get => GridViewAnim.TotalFrames; }

        static bool  IsStopped { get; set; }

        public static void UpdateFrame() 
        {
            if (!IsStopped  && GameLoopHelper.GridView.AllowUpdateFrameOperations())
            {
                var animsByFrame = GridViewAnim.AnimKeyFrames[CurrentFrame];
                var gridVertices = animsByFrame.PositionToMoveTo.Select(s => new Vertex4
                {
                    Position = new Vector4(s, 1f),
                    Color = Color4.LimeGreen,
                }).ToList();

                bool debugIndices = false;
                bool debugNoIndices = true;

                #region Try With Indices Given
                if (debugIndices)
                {
                    var indices = RenderHelper.CalculatesIndicesForVerticesGiven(gridVertices, out List<Vertex4> IndexedVert4s);
                    GameLoopHelper.GridView.Mesh.Update_VAO(IndexedVert4s, indices);
                }
                #endregion
                #region Try without indices
                if (debugNoIndices)
                {
                    var listInd = new List<uint>();
                    for (int i = 0; i < gridVertices.Count; i++)
                    {
                        listInd.Add((uint)i);
                    }

                    GameLoopHelper.GridView.Mesh.Update_VAO(gridVertices, listInd);
                }
                #endregion


                GameLoopHelper.GridView.UpdateFrame();
                CurrentFrame++;
                if (CurrentFrame >= TotalFrames)
                {
                    IsStopped = !GridViewAnim.IsLoopingAnim;
                    CurrentFrame = 0;                
                }
            }
        }

    }

    public interface ScriptBehavior
    {
        public virtual void Awake() { }
        public virtual void UpdateFrame() { }
        public virtual void RenderFrame() { }
    }

}
