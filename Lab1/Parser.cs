using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Lab1
{
	public unsafe static class Parser
    {
        public static List<Vector3> Vertices = [];
		public static List<Vector3> Normals = [];
		public static List<Vector2> Textures = [];
        public static List<List<int>> PolygonsVertices = [];
		public static List<List<int>> PolygonsNormals = [];
		public static List<List<int>> PolygonsTextures = [];
		public static Vector4[] ScreenVertices = [];
		public static Vector3[] WorldVertices = [];
		public static Vector3[] WorldNormals = [];

		public static Vector3[,] DiffuseMap, NormalMap, SpecularMap;
		public static int DiffuseMapWidth, DiffuseMapHeight, NormalMapWidth, NormalMapHeight, SpecularMapWidth, SpecularMapHeight;

		public static void ReadFile(string filename, 
			string? diffuseMapPath = null, string? normalMapPath = null, string? specularMapPath = null)
		{
			using var sr = new StreamReader(filename);
			string? line;
			while ((line = sr.ReadLine()) != null)
			{
                line = Regex.Replace(line.Trim(), @"\s+", " ");
                if (line.Length > 0)
                {
                    var split = line.Split(' ');
					var values = split[1..];
					if (split[0] == "v")
					{
                        var floats = values.Where(x => x != "").Select(float.Parse);
						Vertices.Add(new Vector3(floats.ToArray()));
					}
					else if (split[0] == "vt")
					{
						var floats = values.Where(x => x != "").Select(float.Parse);
						Textures.Add(new Vector2(floats.ToArray()));
					}
					else if (split[0] == "vn")
					{
						var floats = values.Where(x => x != "").Select(float.Parse);
						Normals.Add(new Vector3(floats.ToArray()));
					}
					else if (split[0] == "f")
					{
						var vertexIndexes = new List<int>();
						var textureIndexes = new List<int>();
						var normalIndexes = new List<int>();
						foreach (string value in values)
						{
							var indexes = value.Split('/');
							var vertexIndex = int.Parse(indexes[0]);
							var textureIndex = int.Parse(indexes[1]);
							var normalIndex = int.Parse(indexes[2]);
							vertexIndexes.Add(vertexIndex > 0 ? vertexIndex - 1 : Vertices.Count - vertexIndex);
							textureIndexes.Add(textureIndex > 0 ? textureIndex - 1 : Textures.Count - textureIndex);
							normalIndexes.Add(normalIndex > 0 ? normalIndex - 1 : Normals.Count - normalIndex);
						}
						PolygonsVertices.Add(vertexIndexes);
						PolygonsTextures.Add(textureIndexes);
						PolygonsNormals.Add(normalIndexes);
					}
				}
           
			}

            if (diffuseMapPath != null && normalMapPath != null && specularMapPath != null)
			{
                var dt = Task.Run(() => DiffuseMap = BitmapToBytes((Bitmap)Image.FromFile(diffuseMapPath)));
                var nt = Task.Run(() => NormalMap = BitmapToBytes((Bitmap)Image.FromFile(normalMapPath)));
                var st = Task.Run(() => SpecularMap = BitmapToBytes((Bitmap)Image.FromFile(specularMapPath)));
                Task.WaitAll([dt, nt, st]);
				DiffuseMapWidth = DiffuseMap.GetLength(1);
				DiffuseMapHeight = DiffuseMap.GetLength(0);
				NormalMapWidth = NormalMap.GetLength(1);
				NormalMapHeight = NormalMap.GetLength(0);
				SpecularMapWidth = SpecularMap.GetLength(1);
				SpecularMapHeight = SpecularMap.GetLength(0);
            }
		}

		public unsafe static Vector3[,] BitmapToBytes(Bitmap bitmap)
		{
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var bitsPerPixel = Image.GetPixelFormatSize(bitmapData.PixelFormat);

            byte* scan0 = (byte*)bitmapData.Scan0;
            var result = new Vector3[bitmapData.Height, bitmapData.Width];
            for (int i = 0; i < bitmapData.Height; ++i)
            {
                for (int j = 0; j < bitmapData.Width; ++j)
                {
                    byte* pixel = scan0 + i * bitmapData.Stride + j * bitsPerPixel / 8;
                    result[j, i] = new Vector3(pixel[2], pixel[1], pixel[0]);
                }
            }
            return result;
        }
	}
}
