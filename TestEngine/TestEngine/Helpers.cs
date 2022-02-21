using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if USE_NUMERICS
using System.Numerics;
#endif

namespace TestEngine
{
    #region Helpers
    /// <summary>
    /// Import Objects. Currently obj only
    /// </summary>
    public static class ImportHelper
    {
        static string ThisDirectory = $"C:\\Users\\dmd35\\source\\repos\\TestEngine\\TestEngine\\bin\\Debug\\net5.0";

        /// <summary>
        /// Im calling from GlassWPF, so it never finds this directory. One day we can
        /// move all assets to load from GlassWPF instead of this Test Engine project
        /// </summary>
        static void SetThisDirectory()
        {
            Directory.SetCurrentDirectory(ThisDirectory);
        }

        public static Mesh NewLoadObj(string path, string texturePath = "")
        {
            SetThisDirectory();

            Mesh responseMesh = new Mesh();
            var vertices = new List<Vector4>();
            var textureVertices = new List<Vector2>();
            var normals = new List<Vector3>();

            var vertexIndices = new List<uint>();
            var textureIndices = new List<uint>();
            var normalIndices = new List<uint>();


            if (!File.Exists(path))
            {             
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }
            string objName = "";
            try
            {
                objName = path.Split($"\\").Last().Split(".").FirstOrDefault();
            }
            catch(Exception e)
            {
                Console.WriteLine(new List<string>
                {
                    string.Format("Obj Name was not found => {0}", e.Message),
                });                
            }

            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                 

                    List<string> words = new List<string>(streamReader.ReadLine().ToLower().Split(' '));
                    words.RemoveAll(s => s == string.Empty);

                    if (words.Count == 0)
                        continue;

                    string type = words[0];
                    words.RemoveAt(0);

                    switch (type)
                    {
                        // vertex
                        case "v":

                            vertices.Add(new Vector4(float.Parse(words[0]), float.Parse(words[1]),
                                                    float.Parse(words[2]), words.Count < 4 ? 1 : float.Parse(words[3])));
                            break;

                        case "vt":
                            var words0 = float.Parse(words[0]);
                            var words1 = float.Parse(words[1]);
                            //words0 = words0 > 1 ? 1 : words0 < 0 ? 0 : words0;
                            //words1 = words1 > 1 ? 1 : words1 < 0 ? 0 : words1;

                            textureVertices.Add(new Vector2(words0, words1));
                            //words.Count < 3 ? 0 : float.Parse(words[2])));
                            break;

                        case "vn":
                            normals.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;

                        // face
                        case "f":
                            foreach (string w in words)
                            {
                                if (w.Length == 0)
                                    continue;

                                string[] comps = w.Split('/');

                                // subtract 1: indices start from 1, not 0
                                vertexIndices.Add(uint.Parse(comps[0]) - 1);

                                if (comps.Length > 1 && comps[1].Length != 0)
                                    textureIndices.Add(uint.Parse(comps[1]) - 1);

                                if (comps.Length > 2)
                                    normalIndices.Add(uint.Parse(comps[2]) - 1);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            //Revert back to the original (36 uvs in my cube)
            var originalUVs = new List<Vector2>();
            textureIndices.ForEach(ti => originalUVs.Add(textureVertices[(int)ti]));

            //Revert back to the original (36 vertices in my cube)
            var originalV4s = new List<Vector4>();
            vertexIndices.ForEach(ti => originalV4s.Add(vertices[(int)ti]));

            //Revert back to the original (36 vertices in my cube)
            var originalNorms = new List<Vector3>();
            normalIndices.ForEach(ti => originalNorms.Add(normals[(int)ti]));
            List<Vertex4> n_vertices = new List<Vertex4>();

            if (originalV4s.Count == originalUVs.Count && originalV4s.Count == originalNorms.Count)
            {
                for (int i = 0; i < originalV4s.Count; i++)
                {
                    n_vertices.Add(new Vertex4(originalV4s[i], originalUVs[i], originalNorms[i]));
                }
                //NOW we can index them once they are all aligned into the new vertice object.
                //Now the Indices are no longer segrated in the odd way they once were
                responseMesh = RenderHelper.ImportedMeshGet(n_vertices, objName, texturePath);
            }
            else
            {
                Console.Write(new List<string> {
                    "Error: Counts WERE NOT the same;",
                string.Format("\n Original Vector4s (Pos) Count : {0}", originalV4s.Count.ToString()),
                string.Format("\n Original Norms (Face)   Count : {0}", originalV4s.Count.ToString()),
                string.Format("\n Original UVs (Texture)  Count : {0}", originalV4s.Count.ToString()),
                });
            }
            return responseMesh;
        }


    }

    public static class RenderHelper
    {

        /// <summary>
        /// Calculates Indices for the defined mesh.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static Mesh ImportedMeshGet(List<Vertex4> vertices, string objName = "", string textureFilePath = "")
        {
            var indexedVertices = new List<Vertex4>();
            var indices = CalculatesIndicesForVerticesGiven(vertices, out indexedVertices);

            return new Mesh(indexedVertices, indices, textureFilePath, objName);
        }

        /// <summary>
        /// When a mesh is not imported, the Normals are not yet recognized, so we need
        /// to load/calc them. Then the Indexing after that.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="textureFilePath"></param>
        /// <returns></returns>
        public static Mesh CreatedMeshGet(List<Vertex4> vertices, string objName ="", string textureFilePath = "")
        {
           
            var indexedVertices = new List<Vertex4>();
            vertices = CalculateNormalsWithoutIndicesGiven(vertices.ToArray());
            var indices = CalculatesIndicesForVerticesGiven(vertices, out indexedVertices);
            return new Mesh(indexedVertices, indices, textureFilePath, objName);
        }

        /// <summary>
        /// Calculate the Indice list and shortens the vertices' repetive vectors
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="newList"></param>
        /// <returns>Returns Indices AS WELL AS the new list to index from.</returns>
        public static List<uint> CalculatesIndicesForVerticesGiven(IReadOnlyList<Vertex4> vertices, out List<Vertex4> newList)
        {

            var newIndexedList = new List<Vertex4>();
            var indices = new List<uint>();

            for (int i = 0; i < vertices.Count; i++)
            {
                if (!newIndexedList.Contains(vertices[i]))
                {
                    newIndexedList.Add(vertices[i]);
                }
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                var id = newIndexedList.IndexOf(vertices[i]);
                indices.Add((uint)id);
            }
            newList = newIndexedList;

            return indices;
        }

        #region Calculate Normals With Indices
       

        /// <summary>
        /// Returns Normals
        /// </summary>
        /// <param name="vertexData">Needs to be the Indexed List of Vertices, not the untouched massive original.</param>
        /// <param name="indices">The Indices</param>
        /// <returns></returns>
        public static List<Vertex4> CalculateNormalsWithIndicesGiven(Vertex4[] vertexData, IReadOnlyList<uint> indices)
        {
            //Should be array of same size as vertexData
            
            
            if ((indices.Count % 3) != 0)
            {
                throw new ArgumentException($"Expected {nameof(indices)} to be a multiple of 3 as each triangle consists of 3 points.", nameof(indices));
            }
            
            for (int i = 0; i < indices.Count; i += 3)
            {
                //Grab 3 indexes
                var cornerAIndex = (int)indices[i];
                var cornerBIndex = (int)indices[i + 1];
                var cornerCIndex = (int)indices[i + 2];

                //Use those 3 indexes to grab the vertexes
                var cornerA = vertexData[cornerAIndex];
                var cornerB = vertexData[cornerBIndex];
                var cornerC = vertexData[cornerCIndex];

                //Calculate for Normals
                var ab = cornerB.Position - cornerA.Position;
                var ac = cornerC.Position - cornerA.Position;

                var normal = Vector3.Cross(ab.Xyz, ac.Xyz);
                normal.Normalize();
                vertexData[cornerAIndex].Normal += normal;
                vertexData[cornerBIndex].Normal += normal;
                vertexData[cornerCIndex].Normal += normal;
            }

            //.ForEach(obj => obj.Normal.Normalize())
            return vertexData.ToList();
        }

        #endregion


        #region Create 
        //[Obsolete(message: "Old AF. Use new.")]
        //public static uint[] CalculatesIndices(List<Vertex> vertices, out Vertex[] newList)
        //{

        //    var newIndexedList = new List<Vertex>();
        //    var indices = new List<uint>();

        //    for (int i = 0; i < vertices.Count; i++)
        //    {
        //        if (!newIndexedList.Contains(vertices[i]))
        //        {
        //            newIndexedList.Add(vertices[i]);
        //        }
        //    }
        //    for (int i = 0; i < vertices.Count; i++)
        //    {
        //        var id = newIndexedList.IndexOf(vertices[i]);
        //        indices.Add((uint)id);
        //    }
        //    newList = newIndexedList.ToArray();

        //    return indices.ToArray();
        //}

        /// <summary>
        /// No Indices, so It assigns the faces to the Vertex Array, and returns it. 
        /// I guess the return is redundant considering it is directly using the array.
        /// The return is just in practice I suppose.
        /// </summary>
        /// <param name="vertices">Non-Indexed List of Vertices</param>
        /// <returns></returns>
        public static List<Vertex4> CalculateNormalsWithoutIndicesGiven(Vertex4[] vertices)
        {
            
            int points = 3;//Triange = 3 = Vectors/Points
            
            if (vertices.Length % points != 0)
            {
                Console.WriteLine(new List<string> {
                "Expected to be a multiple of 3 as each triangle consists of 3 points.",
                string.Format("Vertice Count: {0}", vertices.Length),
                });
                throw new ArgumentException("Expected to be a multiple of 3 as each triangle consists of 3 points.");
            }
            for (int i = 0; i < vertices.Length; i += points)
            {
                var pos1 = vertices[i].Position.Xyz;
                var pos2 = vertices[i + 1].Position.Xyz;
                var pos3 = vertices[i + 2].Position.Xyz;

                vertices[i].Normal = CalculateNormals(pos1, pos2, pos3);                
            }

            return vertices.ToList();
        }

        /// <summary>
        /// Finds The Vector Between 2 Points By Subtracting the x,y,z Coordinates From One Point To Another.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private static Vector3 CalculateNormals(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            // Calculate The Vector From Point 1 To Point 0
            var x1 = v0.X - v1.X;
            var y1 = v0.Y - v1.Y;
            var z1 = v0.Z - v1.Z;

            // Calculate The Vector From Point 2 To Point 1
            var x2 = v1.X - v2.X;
            var y2 = v1.Y - v2.Y;
            var z2 = v1.Z - v2.Z;

            // Compute The Cross Product To Give Us A Surface Normal
            var rX = (y1 * z2) - (z1 * y2);// Cross Product For Y - Z
            var rY = (z1 * x2) - (x1 * z2);// Cross Product For X - Z
            var rZ = (x1 * y2) - (y1 * x2);// Cross Product For X - y
            return ReduceToUnit(new Vector3(rX, rY, rZ));
        }


        static Vector3 ReduceToUnit(Vector3 vector3)
        {
            float length = (float)MathHelper.Sqrt((vector3.X * vector3.X) + (vector3.Y * vector3.Y) + (vector3.Z * vector3.Z));
            length = length == 0 ? 1 : length;
            vector3.X /= length;
            vector3.Y /= length;
            vector3.Z /= length;
            return vector3;
        }
        #endregion
    }

    public static class InputHelper
    {
        public static XKeyInputEnums XKeyView = XKeyInputEnums.SolidNormals;

        public static void ChangeXKeyStatus(XKeyInputEnums inputEnum)
        {
            XKeyView = inputEnum;
        }

    }

    /// <summary>
    /// All our inputs for the Keyboard are here.
    /// It is up to the GameLoop/RenderLoop to figure out how to handle them.
    /// But they should call these methods instead of taking up space.
    /// </summary>
    public static class KeyBoardHandler
    {
        /// <summary>
        /// Handles keyboard shortcuts.
        /// <para>Returns true or false on whether to EXIT application.</para>
        /// </summary>
        /// <param name="e"></param>
        /// <param name="inputKey"></param>
        /// <returns></returns>
        public static bool HandleEditorShortcutsReturnExitCode(FrameEventArgs e, KeyboardState inputKey)
        {
            if (inputKey.IsKeyDown(Keys.Escape))
            {
                return true;
            }
            else if (inputKey.IsKeyDown(Keys.X))
            {
                var status = InputHelper.XKeyView;
                switch (status)
                {
                    case XKeyInputEnums.SolidNormals:
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                        InputHelper.ChangeXKeyStatus(XKeyInputEnums.XRay); break;
                    case XKeyInputEnums.XRay:
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        InputHelper.ChangeXKeyStatus(XKeyInputEnums.VertView); break;
                    case XKeyInputEnums.VertView:
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
                        InputHelper.ChangeXKeyStatus(XKeyInputEnums.SolidNormals); break;
                    default: break;
                }
            }
            else if (inputKey.IsKeyPressed(Keys.B))
            {
                Debugger.Break();
            }
            return false;
        }


        #region Editor Movement and View Camera

        private static bool _firstMove = true;

        private static Vector2 _lastPos;

        public static void EditorInputCameraLookAround(FrameEventArgs e, KeyboardState input)
        {           

            const float CameraSpeed = 1.5f;

            if (input.IsKeyDown(Keys.W))
            {
                GameLoopHelper.Camera.Position += GameLoopHelper.Camera.Front * CameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                GameLoopHelper.Camera.Position -= GameLoopHelper.Camera.Front * CameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                GameLoopHelper.Camera.Position -= GameLoopHelper.Camera.Right * CameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                GameLoopHelper.Camera.Position += GameLoopHelper.Camera.Right * CameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                GameLoopHelper.Camera.Position += GameLoopHelper.Camera.Up * CameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                GameLoopHelper.Camera.Position -= GameLoopHelper.Camera.Up * CameraSpeed * (float)e.Time; // Down
            }

        }

        public static void EditorInputMovement(MouseState mouse)
        {
            const float sensitivity = 0.2f;

            if (_firstMove) // This bool variable is initially set to true.
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the GameLoopHelper.Camera pitch and yaw (we clamp the pitch in the GameLoopHelper.Camera class)
                GameLoopHelper.Camera.Yaw += deltaX * sensitivity;
                GameLoopHelper.Camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
        }

        #endregion
    }

    #endregion
}
