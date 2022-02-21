using OpenTK;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Diagnostics;
//using LightBearer;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            GameEngine.RunApplication();
        }
    }

    #region Test Game
    public class GlassRenderer : IDisposable
    {
        Shader AmbientLightShader;
        Shader MainShader;
        int Width, Height;
        GameWindow Window;    
        readonly string CrateTex = @"Components\Resources\Metal_Embossed.bmp";

        readonly string Vert4 = @"Components\Shaders\Vertex4_Vertices.vert";
        readonly string Frag4 = @"Components\Shaders\Vertex4_Fragments.frag";
        readonly string LightingFrag = @"Components\Shaders\Lighting.frag";

        public MyWindow GUIWindow;

        public GlassRenderer(System.Drawing.Size size, string title)
        {
            Width = size.Width;
            Height = size.Height;
            Window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            Window.Title = title;
            Window.Size = new Vector2i(Width, Height);
        }

        public GlassRenderer(int width, int height, string title)
        {

            Width = width;
            Height = height;
            Window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            Window.Title = title;
            Window.Size = new Vector2i(Width, Height);
        }

        public void StartRenderer()
        {
            GUIWindow = new MyWindow(200, 200);
            Window.Load += OnLoad;
            Window.Resize += Resize;
            Window.RenderFrame += OnRenderFrame;
            Window.UpdateFrame += UpdateFrame;
            Window.MouseWheel += OnMouseWheel;
            Window.RenderFrequency = 60.0f;
            Window.UpdateFrequency = 60.0f;
            Window.Run();
        }

        #region Load / Resize

        void OnLoad()
        {
            bool useDungeon = false;/// SeaOfGlassHelper.DungeonStruct != null;

            if (useDungeon)
            {
                //GameLoopHelper.RenderObjects.Add(new Mesh());
                CreateGridView(Color4.LimeGreen, 10, 10);
            }
            else
            {
                GameLoopHelper.GameObjects.Add(new GameObject(ImportHelper.NewLoadObj(@"objs\Box55.obj", CrateTex)));
                CreateGridView(Color4.LimeGreen, 20, 5);
                GameLoopHelper.Lights.Add(new GameObject(ImportHelper.NewLoadObj(@"objs\Box55.obj", CrateTex)));

            }

            GL.Enable(EnableCap.DepthTest);

            //Load Objects?

            AmbientLightShader = new Shader(Vert4, LightingFrag);
            MainShader = new Shader(Vert4, Frag4);


            MainShader.Use();

            Window.VSync = VSyncMode.Off;

            GL.ClearColor(Color.FromArgb(0, 5, 20));

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.PatchParameter(PatchParameterInt.PatchVertices, 3);
            GL.Enable(EnableCap.DepthTest);

            // We also give it the proper aspect ratio.
            GameLoopHelper.Camera = new CameraFP(Vector3.UnitZ * 3, Window.Size.X / (float)Window.Size.Y);

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            Window.CursorVisible = true;
        }

        void Resize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Window.Size.X, Window.Size.X);

            if(Window.Size.X > 0 && Window.Size.Y > 0)
                GameLoopHelper.Camera.AspectRatio = Window.Size.X / Window.Size.Y;
        }

        #endregion

        #region Render/Update Frame

        private double _time;

        void OnRenderFrame(FrameEventArgs e)
        {
            _time += 4.0 * e.Time;
            Window.Title = string.Format("(Vsync: {0}) FPS: {1}", Window.VSync, (1f / e.Time).ToString());

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.FragmentLightingSgix);
            
            
            var lightPosition = new Vector3(2f, 2f, 2f);
            var lightSource = GameLoopHelper.Lights.FirstOrDefault();

            MainShader.Use();
            lightSource.Bind();

            var lightModel = Matrix4.CreateTranslation(lightPosition);// Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
            MainShader.SetMatrix4("model", lightModel);
            MainShader.SetMatrix4("view", GameLoopHelper.Camera.GetViewMatrix());
            MainShader.SetMatrix4("projection", GameLoopHelper.Camera.GetProjectionMatrix());

            lightSource.RenderMesh();
            foreach (var gameObject in GameLoopHelper.GameObjects)
            {
                MainShader.Use();

                gameObject.Bind();

                var model = Matrix4.CreateTranslation(0, 0, 0);// Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(_time));
                MainShader.SetMatrix4("model", model);

                MainShader.SetMatrix4("view", GameLoopHelper.Camera.GetViewMatrix());
                MainShader.SetMatrix4("projection", GameLoopHelper.Camera.GetProjectionMatrix());

                AmbientLightingUse();
                gameObject.RenderMesh();
            }
            
            #region GridView Model

            var gridviewmodel = Matrix4.CreateTranslation(0, 0, 0);

            MainShader.SetMatrix4("model", gridviewmodel);
            MainShader.SetMatrix4("view", GameLoopHelper.Camera.GetViewMatrix());
            MainShader.SetMatrix4("projection", GameLoopHelper.Camera.GetProjectionMatrix());

            AmbientLightingUse();
            GameLoopHelper.GridView.RenderMesh();

            #endregion

            Window.SwapBuffers();
        }

        void UpdateFrame(FrameEventArgs e)
        {
            #region Grid Animation Update
            GameEngine.AnimHandler_UpdateFrame();
            #endregion


            var keyState = Window.KeyboardState;
            Exit(KeyBoardHandler.HandleEditorShortcutsReturnExitCode(e, keyState));
            
            ///If mouse is still focused on window. If we leave then no more cam/movement
            if (Window.IsFocused)
            {
                KeyBoardHandler.EditorInputCameraLookAround(e, keyState);
                KeyBoardHandler.EditorInputMovement(Window.MouseState);
            }

        }

        #endregion

        #region Exit / Zoom

        void Exit(bool isExit)
        {
            if (isExit)
            {
                Console.WriteLine("Have a wonderful day, you beautiful, bright son of a gun.");
                foreach (var obj in GameLoopHelper.GameObjects)
                    obj.Dispose();

                Dispose();
                Window.Close();
            }
        }

        void OnMouseWheel(MouseWheelEventArgs e)
        {
            GameLoopHelper.Camera.Fov -= e.OffsetY;
        }

        #endregion

        #region Ambient Lighting
        
        //Temporary lighting effect
        float ambientLighting = .1f;
        void AmbientLightingUse(Matrix4 m4 = default(Matrix4))
        {
            m4 = m4 == default(Matrix4) ? Matrix4.Identity : m4;

            ambientLighting += ambientLighting > 1 ? -0.9f : .0005f;

            AmbientLightShader.Use();
            
            AmbientLightShader.SetMatrix4("model", m4);

            AmbientLightShader.SetMatrix4("view", GameLoopHelper.Camera.GetViewMatrix());
            AmbientLightShader.SetMatrix4("projection", GameLoopHelper.Camera.GetProjectionMatrix());

            AmbientLightShader.SetFloat("ambientStrength", ambientLighting);

            AmbientLightShader.SetVector3("objectColor", new Vector3(0.5f, 0.5f, 0.31f));
            AmbientLightShader.SetVector3("lightColor", new Vector3(.5f, .5f, .5f));
            AmbientLightShader.SetVector3("lightPos", new Vector3(2f, 2f, 2f));
            AmbientLightShader.SetVector3("viewPos", GameLoopHelper.Camera.Position);
        }

        #endregion

        #region Create Grid

        /// <summary>
        /// Grid and Collor. Sets mesh and render of Gridview
        /// </summary>
        /// <param name="color"></param>
        /// <param name="cellSizeX"></param>
        /// <param name="overrideX">Size Manipulation on the X Axis.</param>
        /// <param name="overrideY">Size Manipulation on the Y Axis.</param>
        /// <param name="overrideZ">Size Manipulation on the Z Axis.</param>
        void CreateGridView(Color4 color, int cellSizeX, int cellSizeZ)
        {

            var gridVertices = new List<Vertex4>();

            for (int x=0;x < cellSizeX; x++)
            {
                for (int z = 0; z < cellSizeZ; z++)
                {
                    gridVertices.AddRange(new List<Vertex4>
                    {
                        new Vertex4(new Vector4(x,     0,  z,     1),  color), //0
                        new Vertex4(new Vector4(x + 1, 0,  z,     1),  color), // 1
                        new Vertex4(new Vector4(x,     0,  z + 1, 1),  color), //2
                        new Vertex4(new Vector4(x,     0,  z + 1, 1),  color), //2
                        new Vertex4(new Vector4(x + 1, 0,  z + 1, 1),  color),//3
                        new Vertex4(new Vector4(x + 1, 0,  z,     1),  color), //1
                    });
                }
            }


            //Cell size cheat
            AnimationFactory.CellSizeX = cellSizeX;
            AnimationFactory.CellSizeZ = cellSizeZ;
            //Give Animation to the Animation Handler
            var gridAnimation = AnimationFactory.GridAnimationGetWavyConnected();
            GridAnimationHandler.GridViewAnim = gridAnimation;

            //Create AnimBreather
            var animBreather = new AnimBreather()
            {
                Animations = new Dictionary<string, Animation>(),
            };
            animBreather.Animations.Add(GridControllerEnums.WaveyMode.ToString(), GridAnimationHandler.GridViewAnim);
            
            
            //Create static Mesh
            var STATIC_MESH = RenderHelper.CreatedMeshGet(gridVertices, "Grid View");
            var gridGO = new GameObject(STATIC_MESH, animBreather);


            
            GameLoopHelper.GridView = gridGO;
        }

        #endregion
       

        public void Dispose()
        {
        }
    }

    #endregion TEST Game
    #region Game Loop
    public static class GameLoopHelper
    {
        public static List<int> VAOs = new List<int>();
        public static List<GameObject> GameObjects = new List<GameObject>();
        public static CameraFP Camera { get; set; }
        public static GameObject GridView { get; set; }

        public static List<GameObject> Lights = new List<GameObject>();

    }
    #endregion

    #region Objects

    public class AmbientLight
    {
        public float AmbientStrength { get; set; }
        public Vector4 Position { get; set; }
        public Vector4 Color { get; set; }

        public AmbientLight() { }
    }

    #endregion
}
