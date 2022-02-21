using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestEngine
{

    /// <summary>
    /// My Second attempt of a mesh class.
    /// </summary>
    public class Mesh : IDisposable
    {
        #region Info

        /// <summary>
        /// Personal Name of Mesh Object
        /// </summary>
        public string Name { get; set; }

        public bool IsInitialized { get => Initialized; }
        protected bool Initialized { get; set; }

        /// <summary>
        /// We wont really need this list, but I have it for debugging only. 
        /// It will slow down memory if all this crap is still stored here.
        /// We wouldnt want this in the end product. Once its stored in the
        /// buffer, we can dispose of this from the stack.
        /// </summary>
        protected List<Vertex4> Vertices { get; set; }

        /// <summary>
        /// This is more accurate than storing the 
        /// whole list of vertices in this class,
        /// since we will need the count at draw time.
        /// </summary>
        protected int VerticeCount { get; set; }
        protected int IndiceCount { get; set; }

        #endregion

        #region VAO/VBO/OTHER Buffers
        protected int _VertexArrayObject { get; set; }
        protected int _IndexBuffer { get; set; }
        protected int _VertexBufferObject { get; set; }
        protected int _ColorBuffer { get; set; }
        protected int _NormalBuffer { get; set; }

        /// <summary>
        /// Index Buffer (Texture2d)
        /// </summary>
        protected int _ITextureBuffer { get; set; }

        /// <summary>
        /// GL.GenTexture();
        /// </summary>
        protected Texture _Texture;

        #endregion

        #region Constructors

        public Mesh()
        {
            Initialized = false;
        }

        /// <summary>
        /// Super basic, not used pften, but works for Grids and stuff
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="name"></param>
        public Mesh(List<Vertex4> vertices, string name = "")
        {
            this.Name = string.IsNullOrWhiteSpace(name)
                ? GenericNameGet()
                : name;
            VerticeCount = vertices.Count();
            IndiceCount = 0;

            Create_VAO(vertices);
        }

        /// <summary>
        /// Used for Objects being imported in. Predetermined values.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indicesV"></param>
        public Mesh(List<Vertex4> vertices, List<uint> indices, string meshName = "")
        {
            this.Name = string.IsNullOrWhiteSpace(meshName)
                ? GenericNameGet()
                : meshName;
            VerticeCount = vertices.Count();
            IndiceCount = indices.Count();
            Create_VAO(vertices, indices);
        }
        public Mesh(List<Vertex4> vertices, List<uint> indices, string texFilePath, string meshName = "")
        {
            this.Name = string.IsNullOrWhiteSpace(meshName)
                ? GenericNameGet()
                : meshName;
            VerticeCount = vertices.Count();
            IndiceCount = indices.Count();

            if(!string.IsNullOrWhiteSpace(texFilePath))
                _Texture = Texture.LoadFromFile(texFilePath);

            Create_VAO(vertices, indices);
        }

        #endregion

        #region Public
        public void Render()
        {
           
            GL.BindBuffer(BufferTarget.ArrayBuffer, _IndexBuffer);
            GL.BindVertexArray(_VertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, IndiceCount, DrawElementsType.UnsignedInt, 0);
                        
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            for (int i = 0; i < 4; i++)
            {
                GL.DisableVertexAttribArray(i);
            }

        }

        public void Dispose()
        {
            GL.DeleteBuffer(_IndexBuffer);
            GL.DeleteVertexArray(_VertexArrayObject);
            GC.SuppressFinalize(this);
            Initialized = false;
        }

        /// <summary>
        /// Call bind before we make any rotation/translation/scale
        /// </summary>
        public void Bind()
        {
            GL.BindVertexArray(_VertexArrayObject);
            if (_Texture != null)
            {
                _Texture.Use(TextureUnit.Texture0);
            }

        }

        #endregion

        #region Protected
       
        /// <summary>
        /// Prepares the VAO and Index Buffer
        /// </summary>
        protected virtual void Create_VAO(List<Vertex4> vertices, List<uint> Indices)
        {
            Initialized = true;

            _VertexBufferObject = GL.GenBuffer(); //VBO Buffer
            _ColorBuffer = GL.GenBuffer();//Colors for Frag?
            _NormalBuffer = GL.GenBuffer();//Normals/Faces => For light FRAG
            _IndexBuffer = GL.GenBuffer();//Indices
            //_ITextureBuffer = useTexIBuffer ? GL.GenBuffer() : -1;

            _VertexArrayObject = GL.GenVertexArray();//VAO Buffer
            
            GL.BindVertexArray(_VertexArrayObject);

            ///Attribue 0 :  Vertex Array => Vector4 Array
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexBufferObject);
            var verts = vertices.Select(s => s.Position).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Count() * Vector4.SizeInBytes), verts, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

            //Attribute 1 : Color Buffer  => Vector4 Array
            GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorBuffer);
            var colors = vertices.Select(s => s.Color).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colors.Length * Vector4.SizeInBytes), colors, BufferUsageHint.StaticDraw);//Here colors is only focusing on size *3 => (R,G,B)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, (IntPtr)16);

            //Attribute 2 : Normals Buffer => Vector3 Array
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalBuffer);
            var normals = vertices.Select(s => s.Normal).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * Vector3.SizeInBytes), normals, BufferUsageHint.StaticDraw);//Here colors is only focusing on size *3 => (R,G,B)
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, (IntPtr)0);

            if (_Texture != null)
            {
                var uvs = vertices.Select(s => s.TexCoords).ToArray();
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(uvs.Length * Vector2.SizeInBytes), uvs, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(4);
                GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, 0, (IntPtr)0);
            }


            if (Indices.Count > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Count * sizeof(uint)), Indices.ToArray(), BufferUsageHint.StaticDraw);//Here colors is only focusing on size *3 => (R,G,B)
            }
          

            GL.BindVertexArray(0);//Zero focusing on UNBINDING whatever was originally just bound
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.DeleteBuffer(_VertexBufferObject);//Delete the Vertex buffer
            GL.DeleteBuffer(_ColorBuffer);//Delete the Color Buffer
            GL.DeleteBuffer(_NormalBuffer);//Delete the Color Buffer

        }

        protected virtual void Create_VAO(List<Vertex4> vertices)
        {
            Create_VAO(vertices, new List<uint>());
        }
        protected string GenericNameGet()
        {
            return string.Format("Untitled_Mesh_{0}", GameLoopHelper.GameObjects.Count());
        }


        public void Update_VAO(List<Vertex4> vertices)
        {
            Update_VAO(vertices, new List<uint>());
        }
        /// <summary>
        /// Prepares the VAO and Index Buffer
        /// </summary>
        public void Update_VAO(List<Vertex4> vertices, List<uint> Indices)
        {
            Initialized = true;

            _VertexBufferObject = GL.GenBuffer(); //VBO Buffer
            _ColorBuffer = GL.GenBuffer();//Colors for Frag?
            _NormalBuffer = GL.GenBuffer();//Normals/Faces => For light FRAG
            _IndexBuffer = GL.GenBuffer();//Indices

            GL.BindVertexArray(_VertexArrayObject);

            ///Attribue 0 :  Vertex Array => Vector4 Array
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexBufferObject);
            var verts = vertices.Select(s => s.Position).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Count() * Vector4.SizeInBytes), verts, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);

            //Attribute 1 : Color Buffer  => Vector4 Array
            GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorBuffer);
            var colors = vertices.Select(s => s.Color).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colors.Length * Vector4.SizeInBytes), colors, BufferUsageHint.DynamicDraw);//Here colors is only focusing on size *3 => (R,G,B)
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, (IntPtr)16);

            //Attribute 2 : Normals Buffer => Vector3 Array
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalBuffer);
            var normals = vertices.Select(s => s.Normal).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * Vector3.SizeInBytes), normals, BufferUsageHint.DynamicDraw);//Here colors is only focusing on size *3 => (R,G,B)
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, (IntPtr)0);

            if (_Texture != null)
            {
                var uvs = vertices.Select(s => s.TexCoords).ToArray();
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(uvs.Length * Vector2.SizeInBytes), uvs, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(4);
                GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, 0, (IntPtr)0);
            }


            if (Indices.Count > 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Count * sizeof(uint)), Indices.ToArray(), BufferUsageHint.StaticDraw);//Here colors is only focusing on size *3 => (R,G,B)
            }


            GL.BindVertexArray(0);//Zero focusing on UNBINDING whatever was originally just bound
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.DeleteBuffer(_VertexBufferObject);//Delete the Vertex buffer
            GL.DeleteBuffer(_ColorBuffer);//Delete the Color Buffer
            GL.DeleteBuffer(_NormalBuffer);//Delete the Color Buffer
        }
        #endregion

    }

    /// <summary>
    /// Stores a single frame state of the mesh
    /// </summary>
    public class MeshAnimation : Mesh
    {
       


    }

}
