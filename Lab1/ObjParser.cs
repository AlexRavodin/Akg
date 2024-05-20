using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Lab1
{
    static class ObjParser
    {
        public static List<Vector3> Vertices = [];
        public static List<int[]> Polygons = [];
        public static List<Vector3> Normals = [];
        public static List<Vector2> Textures = [];

        public static Vector4[] screenVertices;
        public static Vector3[] worldVertices;
        public static Vector3[] worldNormals;

        public static Color[,] DiffuseMap, SpecularMap, NormalMap;
        public static int DiffuseMapWidth, DiffuseMapHeight, SpecularMapWidth, SpecularMapHeight, NormalMapWidth, NormalMapHeight;

        private static readonly string[] verticesTypes = ["v", "vt", "vn", "f"];

        public static void ReadFile(string filePath, string diffuseMapPath, string mirrorMapPath, string normalMapPath)
        {
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    var vertices = sr.ReadToEnd().Split('\n').ToList();

                    var temp = vertices
                        .Select(x => Regex.Replace(x.TrimEnd(), @"\s+", " ").Split(' '))
                        .Where(x => verticesTypes.Any(x[0].Contains)).ToArray();

                    Vertices = temp
                        .Where(x => x[0] == "v")
                        .Select(x => x.Skip(1).ToArray())
                        .Select(x => new Vector3(Array.ConvertAll(x, float.Parse))).ToList();

                    Normals = temp
                        .Where(x => x[0] == "vn")
                        .Select(x => x.Skip(1).ToArray())
                        .Select(x => new Vector3(Array.ConvertAll(x, float.Parse))).ToList();

                    Textures = temp
                        .Where(x => x[0] == "vt")
                        .Select(x => x.Skip(1).ToArray())
                        .Select(x => new Vector2(Array.ConvertAll(x, float.Parse))).ToList();

                    var faces = vertices.Where(x => x.StartsWith('f') == true);
                    foreach (string str in faces)
                    {
                        string pre = str.Remove(0, 1);
                        string[] buf = pre.Trim().Split(['/', ' ']);
                        int length = buf.Length;
                        if (buf.Length == 3)
                        {
                            length *= 3;
                        }
                        int[] res = new int[length];
                        for (int i = 0; i < length; i++)
                        {
                            if (buf.Length == length)
                            {
                                if (buf[i] == "")
                                {
                                    res[i] = 0;
                                }
                                else
                                {
                                    res[i] = int.Parse(buf[i]);
                                }
                            }
                            else
                            {
                                if (i % 3 == 0)
                                {
                                    res[i] = int.Parse(buf[i / 3]);
                                }
                                else
                                {
                                    res[i] = 0;
                                }
                            }
                        }

                        Polygons.Add(res);
                    }
                }
                var diffuse = (Bitmap)Bitmap.FromFile(diffuseMapPath);
                DiffuseMapWidth = diffuse.Width;
                DiffuseMapHeight = diffuse.Height;
                var specular = (Bitmap)Bitmap.FromFile(mirrorMapPath);
                SpecularMapWidth = specular.Width;
                SpecularMapHeight = specular.Height;
                var normal = (Bitmap)Bitmap.FromFile(normalMapPath);
                NormalMapWidth = normal.Width;
                NormalMapHeight = normal.Height;
                Task[] tasks = [
                    Task.Run(() => DiffuseMap = BitmapToBytes(diffuse)),
                    Task.Run(() => SpecularMap = BitmapToBytes(specular)),
                    Task.Run(() => NormalMap = BitmapToBytes(normal)),
                ];
                Task.WaitAll(tasks);

                static Color[,] BitmapToBytes(Bitmap bitmap)
                {
                    var result = new Color[bitmap.Height, bitmap.Width];
                    for (int i = 0; i < bitmap.Height; ++i)
                    {
                        for (int j = 0; j < bitmap.Width; ++j)
                        {
                            result[j, i] = bitmap.GetPixel(j, i);
                        }
                    }
                    return result;
                }
            }
            catch (IOException e)
            {
                MessageBox.Show("The file could not be read:\n\n" + e.Message);
            }
        }
    }
}
