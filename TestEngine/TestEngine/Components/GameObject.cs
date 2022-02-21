using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEngine
{
    public class GameObject : IDisposable
    {

        bool AllowUpdate = false;

        public AnimBreather AnimBreather { get; set; }
        public Mesh Mesh { get; set; }
        public bool IsInitialized { get => Initialized; }
        private bool Initialized { get; set; }
        public GameObject() { }
        public GameObject(Mesh mesh, AnimBreather animBreather = null) {
            Mesh = mesh;
            Initialized = mesh.IsInitialized;
            AnimBreather = animBreather;
        }

        
        /// <summary>
        /// Render the mesh VAO. AllowUpdate = true;
        /// </summary>
        public void RenderMesh() {
            Mesh.Render();
            AllowUpdate = true;
        }

        /// <summary>
        /// We dont want to update faster than we render.
        /// This will tell us if the GO has been Rendered(See Method RenderMesh())
        /// </summary>
        /// <returns></returns>
        public bool AllowUpdateFrameOperations()
        {
            return AllowUpdate;
        }

        public void UpdateFrame()
        {
            AllowUpdate = false;
        }
        
        public void Bind()
        {
            Mesh.Bind();
        }
        public void Dispose()
        {
            Mesh.Dispose();
            Initialized = false;
            GC.SuppressFinalize(this);
        }

    }
}
