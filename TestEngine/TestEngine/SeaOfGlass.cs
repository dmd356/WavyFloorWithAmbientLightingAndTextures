//using LightBearer;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestEngine
{
   

    //public static class SeaOfGlassHelper
    //{
    //    static float colorR = 0.2f;
    //    static float colorG = 0;
    //    static float colorB = 0;
    //    public static DungeonStruct DungeonStruct = null;

    //    public static List<Vertex> GLWalls;


    //    public static List<Vertex> OpenTKRoomData()
    //    {
    //        if (DungeonStruct != null)
    //        {
    //            foreach (var cornerstone in DungeonStruct.MasterBranch.DungeonFloor.CornerStones)
    //            {
    //                var dungeonRoom = DungeonStruct.MasterBranch.DungeonRooms.FirstOrDefault(fod => fod.DungeonRoomId == cornerstone.DungeonRoomId);
    //                ReadRoomData(dungeonRoom, cornerstone);
    //            }

    //        }
    //        return GLWalls;
    //    }
    //    private static void ReadRoomData(DungeonRoom dRoom, DungeonFloorRoomCornerStone cornerStone)
    //    {
    //        var globalX = cornerStone.GlobalCoords.X;
    //        var globalY = cornerStone.GlobalCoords.Y;

    //        var newGrps = new List<WallVerticeGroup>();

    //        foreach(var grp in dRoom.RoomTileMap.BaseWallVerticeGroups)
    //        {
    //            var newGrp = new WallVerticeGroup();

    //            grp.Walls.ForEach(wall => newGrp.Walls.Add(new Wall()
    //            {
    //                BlockX = wall.BlockX + (globalX - cornerStone.RoomSize.X),
    //                BlockY = wall.BlockY + (globalY - cornerStone.RoomSize.Y),
    //                BlockZ = wall.BlockZ
    //            }));
                
    //            var zedHeight = dRoom.DungeonRoomAddress.ArchitectureDetails.RoomSize.Z;
    //            zedHeight = zedHeight > 0 ? zedHeight : 5;
    //            CreateWallModel(newGrp, zedHeight);
    //        }
    //    }

    //    private static void CreateWallModel(WallVerticeGroup vertGrp, int zedHeight)
    //    {           
    //        if (vertGrp.Walls.Count == 2)
    //        { 
    //            GLWalls = GLWalls ?? new List<Vertex>();

    //            GLWalls.AddRange(new List<Vertex> {
    //                    new Vertex(new Vector4(vertGrp.Walls[0].BlockX,vertGrp.Walls[0].BlockZ,vertGrp.Walls[0].BlockY, 1), new Color4(colorR, colorG, colorB, 1)),
    //                    new Vertex(new Vector4(vertGrp.Walls[1].BlockX,vertGrp.Walls[1].BlockZ,vertGrp.Walls[1].BlockY, 1), new Color4(colorR, colorG, colorB, 1)),
    //                    new Vertex(new Vector4(vertGrp.Walls[0].BlockX,zedHeight, vertGrp.Walls[0].BlockY, 1), new Color4(colorR, colorG, colorB, 1)),
    //                    new Vertex(new Vector4(vertGrp.Walls[0].BlockX,zedHeight, vertGrp.Walls[0].BlockY, 1), new Color4(colorR, colorG, colorB, 1)),
    //                    new Vertex(new Vector4(vertGrp.Walls[1].BlockX,zedHeight,vertGrp.Walls[1].BlockY, 1), new Color4(colorR, colorG, colorB, 1)),
    //                    new Vertex(new Vector4(vertGrp.Walls[1].BlockX,vertGrp.Walls[1].BlockZ,vertGrp.Walls[1].BlockY, 1), new Color4(colorR, colorG, colorB, 1)),
    //                });
    //            SetColorsForWallRendering();
    //        }
    //    }

    //    private static void SetColorsForWallRendering()
    //    {
    //        if(colorR > .9f)
    //        {
    //            colorR = 0;
    //            colorG = .2f;
    //        }
    //        if (colorG > .9f)
    //        {
    //            colorG = 0;
    //            colorB = .2f;
    //        }
    //        if (colorB > .9f)
    //        {
    //            colorB = 0;
    //            colorR = .2f;
    //        }

    //        colorR += (colorR > 0 ? .2f : 0);
    //        colorG += (colorG > 0 ? .2f : 0);
    //        colorR += (colorR > 0 ? .2f : 0);

    //    }


    //}
 
    #region Camera
    public interface ICamera
    {
        Matrix4 LookAtMatrix { get; }
        void Update(double time, double delta);
    }

    public class StaticCamera : ICamera
    {
        public Matrix4 LookAtMatrix { get; }
        /// <summary>
        /// Defaults to (0,0,0) of Matrix4
        /// </summary>
        public StaticCamera()
        {
            Vector3 position;
            position.X = 0;
            position.Y = 0;
            position.Z = 0;
            LookAtMatrix = Matrix4.LookAt(position, -Vector3.UnitZ, Vector3.UnitY);
        }

        public StaticCamera(Vector3 position, Vector3 target)
        {
            LookAtMatrix = Matrix4.LookAt(position, target, Vector3.UnitY);
        }
        public void Update(double time, double delta) { }

    }


    //public class ThirdPersonCamera : ICamera
    //{
    //    public Matrix4 LookAtMatrix { get; private set; }
    //    private readonly AGameObject _target;
    //    private readonly Vector3 _offset;

    //    public ThirdPersonCamera(AGameObject target)
    //        : this(target, Vector3.Zero)
    //    { }
    //    public ThirdPersonCamera(AGameObject target, Vector3 offset)
    //    {
    //        _target = target;
    //        _offset = offset;
    //    }

    //    public void Update(double time, double delta)
    //    {
    //        LookAtMatrix = Matrix4.LookAt(
    //            new Vector3(_target.Position) + (_offset * new Vector3(_target.Direction)),
    //            new Vector3(_target.Position),
    //            Vector3.UnitY);
    //    }
    //}
    #endregion


}
