using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Lab1
{

	public static class Transformations
	{
		public static Vector3 Eye = new(0, 30, 100);
		private static Vector3 target = new(0, 30, 0);
		private static Vector3 up = new(0, 1, 0);
		public static Vector3 LightSource = new(0, 30, 180);

		public static float Width { get; set; } = 1;
		public static float Height { get; set; } = 1;

		private static float x_min = 0;
		private static float y_min = 0;
		private static float near = 1;
		private static float far = 100;
		private static float fov = MathF.PI / 4;

		public static Matrix4x4 Viewport = new(
			Width / 2, 0, 0, 0,
			0, -Height / 2, 0, 0,
			0, 0, 1, 0,
			x_min + Width / 2, y_min + Height / 2, 0, 1
		);

		public static void UpdateViewport()
		{
			Viewport.M11 = Width / 2;
			Viewport.M41 = x_min + Width / 2;
			Viewport.M22 = -Height / 2;
			Viewport.M42 = y_min + Height / 2;
		}

		public static void TransformVectors(Vector3 angle, float scaleValue, Vector3 movement)
		{

            //Parser.ScreenVertices = new Vector4[Parser.Vertices.Count];
            //Parser.WorldVertices = new Vector3[Parser.Vertices.Count];
            //Parser.WorldNormals = new Vector3[Parser.Normals.Count];

            var view = Matrix4x4.CreateLookAt(Eye, target, up);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, Width / Height, near, far);
            var scale = Matrix4x4.CreateScale(scaleValue);
            var rotation = Matrix4x4.CreateFromYawPitchRoll(angle.Y, angle.X, angle.Z);
            var translation = Matrix4x4.CreateTranslation(movement);

            var world = scale * rotation * translation;
            var transformation = world * view * projection;

            //for (int i = 0; i < Parser.Vertices.Count; i++)
            //{
            //	Parser.WorldVertices[i] = Vector3.Transform(Parser.Vertices[i], world);
            //	Parser.ScreenVertices[i] = Vector4.Transform(Parser.Vertices[i], transformation);
            //	Parser.ScreenVertices[i] /= Parser.ScreenVertices[i].W;
            //	Parser.ScreenVertices[i] = Vector4.Transform(Parser.ScreenVertices[i], Viewport);
            //}
            //for (int i = 0; i < Parser.Normals.Count; i++)
            //{
            //	Parser.WorldNormals[i] = (Vector3.Transform(Parser.Normals[i], world));
            //}

            ObjParser.screenVertices = new Vector4[ObjParser.Vertices.Count];
            ObjParser.worldVertices = new Vector3[ObjParser.Vertices.Count];
            ObjParser.worldNormals = new Vector3[ObjParser.Normals.Count];

            if (ObjParser.Vertices.Count > 0)
            {
                Parallel.ForEach(Partitioner.Create(0, ObjParser.Vertices.Count), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        ObjParser.worldVertices[i] = Vector3.Transform(ObjParser.Vertices[i], world);
                        ObjParser.screenVertices[i] = Vector4.Transform(ObjParser.Vertices[i], transformation);
                        ObjParser.screenVertices[i] /= ObjParser.screenVertices[i].W;
                        ObjParser.screenVertices[i] = Vector4.Transform(ObjParser.screenVertices[i], Viewport);
                    }
                }
                );

                Parallel.ForEach(Partitioner.Create(0, ObjParser.Normals.Count), range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        ObjParser.worldNormals[i] = Vector3.TransformNormal(ObjParser.Normals[i], world);
                    }
                }
                );
            }
        }
	}
}
