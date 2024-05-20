using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.IsolatedStorage;
using System.IO.Packaging;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Xaml.Schema;
using Color = System.Windows.Media.Color;

namespace Lab1
{
    public unsafe partial class MainWindow : Window
    {
        private Vector3 angle = Vector3.Zero;
        private float scale = 2f;
        private const float scale_level = 0.1f;
        private Vector3 movement = new(0, 0, 0);
        private float[,] zBuffer = new float[1, 1];
        private byte* bitmapBackBuffer;
        private int bitmapStride;
        private int bitmapHeight;
        private int bitmapWidth;
        private int bitmapDepth;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += delegate
            {
                Transformations.Width = (float)img.ActualWidth;
                Transformations.Height = (float)img.ActualHeight;
                var path = "..\\..\\..\\objects\\box\\";
                //Parser.ReadFile(path + "Model.obj", path + "diffuse.png", path + "normal.png", path + "specular.png");
                //Parser.ReadFile("..\\..\\..\\objects\\cat.obj");
                ObjParser.ReadFile(path + "Model.obj", path + "diffuse.png", path + "normal.png", path + "specular.png");
                Transformations.UpdateViewport();
                Transformations.TransformVectors(angle, scale, movement);
                WriteableBitmap bitmap = new((int)Transformations.Width, (int)Transformations.Height, 96, 96, PixelFormats.Bgra32, null);
                zBuffer = new float[bitmap.PixelHeight, bitmap.PixelWidth];
                DrawModel();
            };
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Up:
                    movement.Y += 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.Down:
                    movement.Y -= 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.Left:
                    movement.X -= 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.Right:
                    movement.X += 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad0:
                    movement.Z -= 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad1:
                    movement.Z += 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.W:
                    Transformations.LightSource.Y += 1;
                    DrawModel();
                    break;
                case System.Windows.Input.Key.S:
                    Transformations.LightSource.Y -= 1;
                    DrawModel();
                    break;
                case System.Windows.Input.Key.A:
                    Transformations.LightSource.X -= 1;
                    DrawModel();
                    break;
                case System.Windows.Input.Key.D:
                    Transformations.LightSource.X += 1;
                    DrawModel();
                    break;
                case System.Windows.Input.Key.Q:
                    Transformations.LightSource.Z -= 1;
                    DrawModel();
                    break;
                case System.Windows.Input.Key.E:
                    Transformations.LightSource.Z += 1;
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad8:
                    angle.X += 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad5:
                    angle.X -= 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad4:
                    angle.Y += 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad6:
                    angle.Y -= 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad7:
                    angle.Z += 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.NumPad9:
                    angle.Z -= 0.1f;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.Add:
                    scale += scale_level;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
                case System.Windows.Input.Key.Subtract:
                    scale -= scale_level;
                    Transformations.TransformVectors(angle, scale, movement);
                    DrawModel();
                    break;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (img.ActualWidth > 0 && img.ActualHeight > 0)
            {
                Transformations.Width = (float)img.ActualWidth;
                Transformations.Height = (float)img.ActualHeight;
                Transformations.UpdateViewport();
                Transformations.TransformVectors(angle, scale, movement);
                DrawModel();
            }
            else
            {
                img.Source = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            }
        }

        public void DrawModel()
        {
            WriteableBitmap bitmap = new((int)Transformations.Width, (int)Transformations.Height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.Lock();

            bitmapBackBuffer = (byte*)bitmap.BackBuffer;
            bitmapStride = bitmap.BackBufferStride;
            bitmapHeight = bitmap.PixelHeight;
            bitmapWidth = bitmap.PixelWidth;
            bitmapDepth = bitmap.Format.BitsPerPixel;

            //foreach (List<int> vertices in Parser.Polygons)
            //{
            //	int i;
            //	for (i = 0; i < vertices.Count - 1; i++)
            //	{
            //		DrawVector(bitmap, Parser.ScreenVertices[vertices[i]], Parser.ScreenVertices[vertices[i + 1]]);
            //	}
            //	DrawVector(bitmap, Parser.ScreenVertices[vertices[i]], Parser.ScreenVertices[vertices[0]]);
            //}

            //DrawModelFlatShading();

            //DrawModelPhongShading();

            DrawModelTexturing();

            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();
            img.Source = bitmap;
        }

        //public void DrawModelFlatShading()
        //{
        //    for (int i = 0; i < zBuffer.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < zBuffer.GetLength(1); j++)
        //        {
        //            zBuffer[i, j] = float.MaxValue;
        //        }
        //    }

        //    Parallel.For(0, Parser.PolygonsVertices.Count, i =>
        //    {
        //        var polygon = Parser.PolygonsVertices[i];
        //        Vector3[] worldVertices =
        //        [
        //            Parser.WorldVertices[polygon[0]],
        //            Parser.WorldVertices[polygon[1]],
        //            Parser.WorldVertices[polygon[2]]
        //        ];
        //        var side1 = worldVertices[1] - worldVertices[0];
        //        var side2 = worldVertices[2] - worldVertices[0];
        //        var normal = Vector3.Normalize(Vector3.Cross(side1, side2));
        //        var lightVector = Vector3.Normalize(Transformations.LightSource - worldVertices[0]);
        //        var intensity = Vector3.Dot(normal, lightVector);
        //        var polygonColor = Color.Multiply(Colors.White, intensity);

        //        Vector4[] screenVertices =
        //        [
        //            Parser.ScreenVertices[polygon[0]],
        //            Parser.ScreenVertices[polygon[1]],
        //            Parser.ScreenVertices[polygon[2]]
        //        ];

        //        if (screenVertices[0].Y > screenVertices[1].Y)
        //        {
        //            (screenVertices[0], screenVertices[1]) = (screenVertices[1], screenVertices[0]);
        //        }
        //        if (screenVertices[0].Y > screenVertices[2].Y)
        //        {
        //            (screenVertices[0], screenVertices[2]) = (screenVertices[2], screenVertices[0]);
        //        }
        //        if (screenVertices[1].Y > screenVertices[2].Y)
        //        {
        //            (screenVertices[1], screenVertices[2]) = (screenVertices[2], screenVertices[1]);
        //        }

        //        var screenDelta01 = (screenVertices[1] - screenVertices[0]) / (screenVertices[1].Y - screenVertices[0].Y);
        //        var screenDelta02 = (screenVertices[2] - screenVertices[0]) / (screenVertices[2].Y - screenVertices[0].Y);
        //        var screenDelta12 = (screenVertices[2] - screenVertices[1]) / (screenVertices[2].Y - screenVertices[1].Y);

        //        var minY = Math.Max((int)MathF.Ceiling(screenVertices[0].Y), 0);
        //        var maxY = Math.Min((int)MathF.Ceiling(screenVertices[2].Y), bitmapHeight);
        //        var middleY = Math.Clamp((int)MathF.Ceiling(screenVertices[1].Y), 0, bitmapHeight);

        //        for (var y = minY; y < middleY; y++)
        //        {
        //            var screenLeft = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta01;
        //            var screenRight = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta02;
        //            InterpolateX(y, screenLeft, screenRight, polygonColor);
        //        }

        //        for (var y = middleY; y < maxY; y++)
        //        {
        //            var screenLeft = screenVertices[1] + (y - screenVertices[1].Y) * screenDelta12;
        //            var screenRight = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta02;
        //            InterpolateX(y, screenLeft, screenRight, polygonColor);
        //        }

        //        void InterpolateX(int y, Vector4 screenLeft, Vector4 screenRight, Color color)
        //        {
        //            if (screenLeft.X > screenRight.X)
        //            {
        //                (screenLeft, screenRight) = (screenRight, screenLeft);
        //            }

        //            int minX = Math.Max((int)MathF.Ceiling(screenLeft.X), 0);
        //            int maxX = Math.Min((int)MathF.Ceiling(screenRight.X), bitmapWidth);

        //            var screenDelta = (screenRight - screenLeft) / (screenRight.X - screenLeft.X);

        //            for (int x = minX; x < maxX; x++)
        //            {
        //                var pointScreen = screenLeft + (x - screenLeft.X) * screenDelta;
        //                if (pointScreen.Z <= zBuffer[y, x])
        //                {
        //                    zBuffer[y, x] = pointScreen.Z;
        //                    DrawPixel(x, y, color);
        //                }
        //            }
        //        }
        //    });


        //}

        //public void DrawModelPhongShading()
        //{
        //    for (int i = 0; i < zBuffer.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < zBuffer.GetLength(1); j++)
        //        {
        //            zBuffer[i, j] = float.MaxValue;
        //        }
        //    }

        //    Parallel.For(0, Parser.PolygonsVertices.Count, i =>
        //    {

        //        var polygon = Parser.PolygonsVertices[i];
        //        Vector4[] screenVertices =
        //        [
        //            Parser.ScreenVertices[polygon[0]],
        //            Parser.ScreenVertices[polygon[1]],
        //            Parser.ScreenVertices[polygon[2]]
        //        ];
        //        Vector3[] worldVertices =
        //        [
        //            Parser.WorldVertices[polygon[0]],
        //            Parser.WorldVertices[polygon[1]],
        //            Parser.WorldVertices[polygon[2]]
        //        ];

        //        var normal0 = Vector3.Normalize(Parser.WorldNormals[Parser.PolygonsNormals[i][0]]);
        //        var normal1 = Vector3.Normalize(Parser.WorldNormals[Parser.PolygonsNormals[i][1]]);
        //        var normal2 = Vector3.Normalize(Parser.WorldNormals[Parser.PolygonsNormals[i][2]]);

        //        if (screenVertices[0].Y > screenVertices[1].Y)
        //        {
        //            (screenVertices[0], screenVertices[1]) = (screenVertices[1], screenVertices[0]);
        //            (worldVertices[0], worldVertices[1]) = (worldVertices[1], worldVertices[0]);
        //            (normal0, normal1) = (normal1, normal0);
        //        }
        //        if (screenVertices[0].Y > screenVertices[2].Y)
        //        {
        //            (screenVertices[0], screenVertices[2]) = (screenVertices[2], screenVertices[0]);
        //            (worldVertices[0], worldVertices[2]) = (worldVertices[2], worldVertices[0]);
        //            (normal0, normal2) = (normal2, normal0);
        //        }
        //        if (screenVertices[1].Y > screenVertices[2].Y)
        //        {
        //            (screenVertices[1], screenVertices[2]) = (screenVertices[2], screenVertices[1]);
        //            (worldVertices[1], worldVertices[2]) = (worldVertices[2], worldVertices[1]);
        //            (normal1, normal2) = (normal2, normal1);
        //        }

        //        var screenDelta01 = (screenVertices[1] - screenVertices[0]) / (screenVertices[1].Y - screenVertices[0].Y);
        //        var worldDelta01 = (worldVertices[1] - worldVertices[0]) / (screenVertices[1].Y - screenVertices[0].Y);
        //        var normalDelta01 = (normal1 - normal0) / (screenVertices[1].Y - screenVertices[0].Y);

        //        var screenDelta02 = (screenVertices[2] - screenVertices[0]) / (screenVertices[2].Y - screenVertices[0].Y);
        //        var worldDelta02 = (worldVertices[2] - worldVertices[0]) / (screenVertices[2].Y - screenVertices[0].Y);
        //        var normalDelta02 = (normal2 - normal0) / (screenVertices[2].Y - screenVertices[0].Y);

        //        var screenDelta12 = (screenVertices[2] - screenVertices[1]) / (screenVertices[2].Y - screenVertices[1].Y);
        //        var worldDelta12 = (worldVertices[2] - worldVertices[1]) / (screenVertices[2].Y - screenVertices[1].Y);
        //        var normalDelta12 = (normal2 - normal1) / (screenVertices[2].Y - screenVertices[1].Y);

        //        var minY = Math.Max((int)MathF.Ceiling(screenVertices[0].Y), 0);
        //        var maxY = Math.Min((int)MathF.Ceiling(screenVertices[2].Y), bitmapHeight);
        //        var middleY = Math.Clamp((int)MathF.Ceiling(screenVertices[1].Y), 0, bitmapHeight);

        //        for (var y = minY; y < middleY; y++)
        //        {
        //            var screenLeft = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta01;
        //            var screenRight = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta02;
        //            var worldLeft = worldVertices[0] + (y - screenVertices[0].Y) * worldDelta01;
        //            var worldRight = worldVertices[0] + (y - screenVertices[0].Y) * worldDelta02;
        //            var normalLeft = normal0 + (y - screenVertices[0].Y) * normalDelta01;
        //            var normalRight = normal0 + (y - screenVertices[0].Y) * normalDelta02;
        //            PhongInterpolateX(y, screenLeft, screenRight, worldLeft, worldRight, normalLeft, normalRight);
        //        }

        //        for (var y = middleY; y < maxY; y++)
        //        {
        //            var screenLeft = screenVertices[1] + (y - screenVertices[1].Y) * screenDelta12;
        //            var screenRight = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta02;
        //            var worldLeft = worldVertices[1] + (y - screenVertices[1].Y) * worldDelta12;
        //            var worldRight = worldVertices[0] + (y - screenVertices[0].Y) * worldDelta02;
        //            var normalLeft = normal1 + (y - screenVertices[1].Y) * normalDelta12;
        //            var normalRight = normal0 + (y - screenVertices[0].Y) * normalDelta02;
        //            PhongInterpolateX(y, screenLeft, screenRight, worldLeft, worldRight, normalLeft, normalRight);
        //        }

        //        void PhongInterpolateX(int y, Vector4 screenLeft, Vector4 screenRight,
        //            Vector3 worldLeft, Vector3 worldRight, Vector3 normalLeft, Vector3 normalRight)
        //        {
        //            if (screenLeft.X > screenRight.X)
        //            {
        //                (screenLeft, screenRight) = (screenRight, screenLeft);
        //                (worldLeft, worldRight) = (worldRight, worldLeft);
        //                (normalLeft, normalRight) = (normalRight, normalLeft);
        //            }
        //            int minX = Math.Max((int)MathF.Ceiling(screenLeft.X), 0);
        //            int maxX = Math.Min((int)MathF.Ceiling(screenRight.X), bitmapWidth);
        //            var screenDelta = (screenRight - screenLeft) / (screenRight.X - screenLeft.X);
        //            var worldDelta = (worldRight - worldLeft) / (screenRight.X - screenLeft.X);
        //            var normalDelta = (normalRight - normalLeft) / (screenRight.X - screenLeft.X);

        //            for (int x = minX; x < maxX; x++)
        //            {
        //                var pointScreen = screenLeft + (x - screenLeft.X) * screenDelta;
        //                var pointWorld = worldLeft + (x - screenLeft.X) * worldDelta;
        //                if (pointScreen.Z <= zBuffer[y, x])
        //                {
        //                    zBuffer[y, x] = pointScreen.Z;

        //                    var normal = normalLeft + (x - screenLeft.X) * normalDelta;
        //                    normal = Vector3.Normalize(normal);

        //                    var ambientColor = new Vector3(5, 0, 50);
        //                    var diffuseColor = new Vector3(10, 0, 100);
        //                    var specularColor = new Vector3(255, 255, 255);
        //                    var shininess = 1000f;

        //                    var color = PhongLighting(pointWorld, normal, ambientColor, diffuseColor, specularColor, shininess);
        //                    DrawPixel(x, y, color);
        //                }
        //            }
        //        }
        //    });
        //}

        private void DrawModelTexturing()
        {
            float?[,] zBuffer = new float?[bitmapHeight, bitmapWidth];
            Parallel.ForEach(ObjParser.Polygons, (int[] vector) =>
            {
                for (int j = 3; j < vector.Length - 3; j += 3)
                {
                    Vector4[] screenVertices = 
                    [ 
                        ObjParser.screenVertices[vector[0] - 1], 
                        ObjParser.screenVertices[vector[j] - 1], 
                        ObjParser.screenVertices[vector[j + 3] - 1] 
                    ];
                    Vector3[] worldVertices = 
                    [ 
                        ObjParser.worldVertices[vector[0] - 1], 
                        ObjParser.worldVertices[vector[j] - 1], 
                        ObjParser.worldVertices[vector[j + 3] - 1] 
                    ];

                    var normal0 = Vector3.Normalize(ObjParser.worldNormals[vector[2] - 1]);
                    var normal1 = Vector3.Normalize(ObjParser.worldNormals[vector[j + 2] - 1]);
                    var normal2 = Vector3.Normalize(ObjParser.worldNormals[vector[j + 5] - 1]);

                    var texture0 = ObjParser.Textures[vector[1] - 1] / screenVertices[0].Z;
                    var texture1 = ObjParser.Textures[vector[j + 1] - 1] / screenVertices[1].Z;
                    var texture2 = ObjParser.Textures[vector[j + 4] - 1] / screenVertices[2].Z;
                    var reciprocal0 = 1 / screenVertices[0].Z;
                    var reciprocal1 = 1 / screenVertices[1].Z;
                    var reciprocal2 = 1 / screenVertices[2].Z;

                    if (screenVertices[0].Y > screenVertices[1].Y)
                    {
                        (screenVertices[0], screenVertices[1]) = (screenVertices[1], screenVertices[0]);
                        (worldVertices[0], worldVertices[1]) = (worldVertices[1], worldVertices[0]);
                        (normal0, normal1) = (normal1, normal0);
                        (texture0, texture1) = (texture1, texture0);
                        (reciprocal0, reciprocal1) = (reciprocal1, reciprocal0);
                    }
                    if (screenVertices[0].Y > screenVertices[2].Y)
                    {
                        (screenVertices[0], screenVertices[2]) = (screenVertices[2], screenVertices[0]);
                        (worldVertices[0], worldVertices[2]) = (worldVertices[2], worldVertices[0]);
                        (normal0, normal2) = (normal2, normal0);
                        (texture0, texture2) = (texture2, texture0);
                        (reciprocal0, reciprocal2) = (reciprocal2, reciprocal0);
                    }
                    if (screenVertices[1].Y > screenVertices[2].Y)
                    {
                        (screenVertices[1], screenVertices[2]) = (screenVertices[2], screenVertices[1]);
                        (worldVertices[1], worldVertices[2]) = (worldVertices[2], worldVertices[1]);
                        (normal1, normal2) = (normal2, normal1);
                        (texture1, texture2) = (texture2, texture1);
                        (reciprocal1, reciprocal2) = (reciprocal2, reciprocal1);
                    }

                    var screenDelta01 = (screenVertices[1] - screenVertices[0]) / (screenVertices[1].Y - screenVertices[0].Y);
                    var worldDelta01 = (worldVertices[1] - worldVertices[0]) / (screenVertices[1].Y - screenVertices[0].Y);
                    var normalDelta01 = (normal1 - normal0) / (screenVertices[1].Y - screenVertices[0].Y);
                    var textureDelta01 = (texture1 - texture0) / (screenVertices[1].Y - screenVertices[0].Y);
                    var reciprocalDelta01 = (reciprocal1 - reciprocal0) / (screenVertices[1].Y - screenVertices[0].Y);

                    var screenDelta02 = (screenVertices[2] - screenVertices[0]) / (screenVertices[2].Y - screenVertices[0].Y);
                    var worldDelta02 = (worldVertices[2] - worldVertices[0]) / (screenVertices[2].Y - screenVertices[0].Y);
                    var normalDelta02 = (normal2 - normal0) / (screenVertices[2].Y - screenVertices[0].Y);
                    var textureDelta02 = (texture2 - texture0) / (screenVertices[2].Y - screenVertices[0].Y);
                    var reciprocalDelta02 = (reciprocal2 - reciprocal0) / (screenVertices[2].Y - screenVertices[0].Y);

                    var screenDelta12 = (screenVertices[2] - screenVertices[1]) / (screenVertices[2].Y - screenVertices[1].Y);
                    var worldDelta12 = (worldVertices[2] - worldVertices[1]) / (screenVertices[2].Y - screenVertices[1].Y);
                    var normalDelta12 = (normal2 - normal1) / (screenVertices[2].Y - screenVertices[1].Y);
                    var textureDelta12 = (texture2 - texture1) / (screenVertices[2].Y - screenVertices[1].Y);
                    var reciprocalDelta12 = (reciprocal2 - reciprocal1) / (screenVertices[2].Y - screenVertices[1].Y);

                    var minY = Math.Max((int)MathF.Ceiling(screenVertices[0].Y), 0);
                    var maxY = Math.Min((int)MathF.Ceiling(screenVertices[2].Y), bitmapHeight);
                    var middleY = Math.Clamp((int)MathF.Ceiling(screenVertices[1].Y), 0, bitmapHeight);

                    Vector4 screenLeft, screenRight;
                    Vector3 worldLeft, worldRight, normalLeft, normalRight;
                    Vector2 textureLeft, textureRight;
                    float reciprocalLeft, reciprocalRight;

                    for (int y = minY; y < middleY; y++)
                    {
                        screenLeft = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta01;
                        screenRight = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta02;
                        worldLeft = worldVertices[0] + (y - screenVertices[0].Y) * worldDelta01;
                        worldRight = worldVertices[0] + (y - screenVertices[0].Y) * worldDelta02;
                        normalLeft = normal0 + (y - screenVertices[0].Y) * normalDelta01;
                        normalRight = normal0 + (y - screenVertices[0].Y) * normalDelta02;
                        textureLeft = texture0 + (y - screenVertices[0].Y) * textureDelta01;
                        textureRight = texture0 + (y - screenVertices[0].Y) * textureDelta02;
                        reciprocalLeft = reciprocal0 + (y - screenVertices[0].Y) * reciprocalDelta01;
                        reciprocalRight = reciprocal0 + (y - screenVertices[0].Y) * reciprocalDelta02;

                        TexturingInterpolateX(screenLeft, screenRight, worldLeft, worldRight,
                            normalLeft, normalRight, textureLeft, textureRight, reciprocalLeft, reciprocalRight, y);
                    }
                    for (int y = middleY; y < maxY; y++)
                    {
                        screenLeft = screenVertices[1] + (y - screenVertices[1].Y) * screenDelta12;
                        screenRight = screenVertices[0] + (y - screenVertices[0].Y) * screenDelta02;
                        worldLeft = worldVertices[1] + (y - screenVertices[1].Y) * worldDelta12;
                        worldRight = worldVertices[0] + (y - screenVertices[0].Y) * worldDelta02;
                        normalLeft = normal1 + (y - screenVertices[1].Y) * normalDelta12;
                        normalRight = normal0 + (y - screenVertices[0].Y) * normalDelta02;
                        textureLeft = texture1 + (y - screenVertices[1].Y) * textureDelta12;
                        textureRight = texture0 + (y - screenVertices[0].Y) * textureDelta02;
                        reciprocalLeft = reciprocal1 + (y - screenVertices[0].Y) * reciprocalDelta12;
                        reciprocalRight = reciprocal0 + (y - screenVertices[0].Y) * reciprocalDelta02;

                        TexturingInterpolateX(screenLeft, screenRight, worldLeft, worldRight,
                            normalLeft, normalRight, textureLeft, textureRight, reciprocalLeft, reciprocalRight, y);
                    }

                   
                    void TexturingInterpolateX(Vector4 screenLeft, Vector4 screenRight, Vector3 worldLeft, Vector3 worldRight,
                        Vector3 normalLeft, Vector3 normalRight, Vector2 textureLeft, Vector2 textureRight, float reciprocalLeft, float reciprocalRight, int y)
                    {
                        if (screenLeft.X > screenRight.X)
                        {
                            (screenLeft, screenRight) = (screenRight, screenLeft);
                            (worldLeft, worldRight) = (worldRight, worldLeft);
                            (normalLeft, normalRight) = (normalRight, normalLeft);
                            (textureLeft, textureRight) = (textureRight, textureLeft);
                        }
                        var normаl = Vector3.Zero;
                        var minX = Math.Max((int)MathF.Ceiling(screenLeft.X), 0);
                        var maxX = Math.Min((int)MathF.Ceiling(screenRight.X), bitmapWidth);

                        var screenDelta = (screenRight - screenLeft) / (screenRight.X - screenLeft.X);
                        var worldDelta = (worldRight - worldLeft) / (screenRight.X - screenLeft.X);
                        var normalDelta = (normalRight - normalLeft) / (screenRight.X - screenLeft.X);
                        var textureDelta = (textureRight - textureLeft) / (screenRight.X - screenLeft.X);
                        var reciprocalDelta = (reciprocalRight - reciprocalLeft) / (screenRight.X - screenLeft.X);

                        for (int x = minX; x < maxX; x++)
                        {
                            var screenPoint = screenLeft + (x - screenLeft.X) * screenDelta;
                            var worldPoint = worldLeft + (x - screenLeft.X) * worldDelta;
                            if (!(screenPoint.Z > zBuffer[y, x]))
                            {
                                zBuffer[y, x] = screenPoint.Z;
                                var normal = (normalLeft + (x - screenLeft.X) * normalDelta);
                                var lightDirection = (worldPoint - Transformations.LightSource);
                                var viewDirection = (worldPoint - Transformations.Eye);
                                var texture = textureLeft + (x - screenLeft.X) * textureDelta;
                                var reciprocal = reciprocalLeft + (x - screenLeft.X) * reciprocalDelta;
                                texture /= reciprocal;
                                texture.X = texture.X > 1 ? 1 : texture.X;
                                texture.Y = texture.Y > 1 ? 1 : texture.Y;
                                //texture = ((Vector2.One - textureDelta) * (textureLeft / screenLeft.Z) +
                                //                    textureDelta * (textureRight / screenRight.Z)) /
                                //                  ((Vector2.One - textureDelta) * (1 / screenLeft.Z) +
                                //                    textureDelta * (1 / screenRight.Z));
                                var diffuseColor = ObjParser.DiffuseMap[
                                        (int)(texture.X * (ObjParser.DiffuseMapWidth - 1)),
                                        (int)((1 - texture.Y) * (ObjParser.DiffuseMapHeight - 1))];
                                var diffuse = new Vector3(diffuseColor.R, diffuseColor.G, diffuseColor.B);

                                var specularColor = ObjParser.SpecularMap[
                                        (int)(texture.X * (ObjParser.SpecularMapWidth - 1)),
                                        (int)((1 - texture.Y) * (ObjParser.SpecularMapHeight - 1))];
                                var specular = new Vector3(specularColor.R, specularColor.G, specularColor.B);
                                
                                var normalColor = ObjParser.NormalMap[
                                        (int)(texture.X * (ObjParser.NormalMapWidth - 1)),
                                        (int)((1 - texture.Y) * (ObjParser.NormalMapHeight - 1))];
                                normаl = new Vector3(normalColor.R, normalColor.G, normalColor.B) / 255;
                                normаl = ((normal * 2) - Vector3.One);

                                var c = PhongLighting(worldPoint, normal, Vector3.Zero, diffuse, Vector3.Zero, 1);
                                DrawPixel(x, y, c);
                            }
                        }
                    }
                }
            });
        }

        private static Color PhongLighting(Vector3 pointWorld, Vector3 normal,
            Vector3 ambientColor, Vector3 diffuseColor, Vector3 specularColor, float shininess)
        {
            var ambientCoef = 0.01f;
            var diffuseCoef = 1f;
            var specularCoef = 1f;
            var lightDirection = Vector3.Normalize(Transformations.LightSource - pointWorld);

            var cosinusLN = Math.Max(0, Vector3.Dot(lightDirection, normal));
            var diffuse = MultiplyColor(diffuseColor, cosinusLN);
            diffuse = MultiplyColor(diffuse, diffuseCoef);

            var reflectedLight = Vector3.Normalize(Vector3.Reflect(-lightDirection, normal));
            var viewDirection = Vector3.Normalize(Transformations.Eye - pointWorld);
            var cosinusRV = Math.Max(0, Vector3.Dot(reflectedLight, viewDirection));
            var specular = MultiplyColor(specularColor, MathF.Pow(cosinusRV, shininess));
            specular = MultiplyColor(specular, specularCoef);

            var color = MultiplyColor(ambientColor, ambientCoef) + diffuse + specular;
            if (color.X > 255) color.X = 255;
            if (color.Y > 255) color.Y = 255;
            if (color.Z > 255) color.Z = 255;
            return Color.FromRgb((byte)color.X, (byte)color.Y, (byte)color.Z);
        }

        private static Vector3 MultiplyColor(Vector3 color, float x)
        {
            color.X = Math.Min(color.X * x, 255);
            color.Y = Math.Min(color.Y * x, 255);
            color.Z = Math.Min(color.Z * x, 255);
            return color;
        }

        private unsafe void DrawPixel(int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < bitmapWidth && y < bitmapHeight)
            {
                byte* pixelPtr = bitmapBackBuffer + y * bitmapStride + x * bitmapDepth / 8;
                pixelPtr[0] = color.B;
                pixelPtr[1] = color.G;
                pixelPtr[2] = color.R;
                pixelPtr[3] = color.A;
            }
        }

        private unsafe void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, Colors.White);
        }

        public void DrawVector(Vector4 p1, Vector4 p2)
        {
            var L = Math.Max(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));

            var x = p1.X;
            var y = p1.Y;
            var deltaX = (p2.X - p1.X) / L;
            var deltaY = (p2.Y - p1.Y) / L;
            DrawPixel((int)Math.Round(x), (int)Math.Round(y));
            for (int i = 0; i < L; i++)
            {
                x += deltaX;
                y += deltaY;
                DrawPixel((int)Math.Round(x), (int)Math.Round(y));
            }
        }
    }
}
