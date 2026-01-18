using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK.Mathematics;

namespace Temple3D
{
    public static class ObjLoader
    {
        public static float[] Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException($"Model not found: {path}");

            List<Vector3> tempVertices = new List<Vector3>();
            List<Vector2> tempUVs = new List<Vector2>();
            List<Vector3> tempNormals = new List<Vector3>();

            List<float> finalData = new List<float>();

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts[0] == "v")
                {
                    tempVertices.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                }
                else if (parts[0] == "vt")
                {
                    tempUVs.Add(new Vector2(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture)));
                }
                else if (parts[0] == "vn")
                {
                    tempNormals.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                }
                else if (parts[0] == "f")
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        var indexParts = parts[i].Split('/');

                        int vIndex = int.Parse(indexParts[0]) - 1;
                        int tIndex = int.Parse(indexParts[1]) - 1;
                        int nIndex = int.Parse(indexParts[2]) - 1;

                        Vector3 v = tempVertices[vIndex];
                        Vector3 n = tempNormals[nIndex];
                        Vector2 t = tempUVs[tIndex];

                        finalData.Add(v.X); finalData.Add(v.Y); finalData.Add(v.Z);
                        finalData.Add(n.X); finalData.Add(n.Y); finalData.Add(n.Z);
                        finalData.Add(t.X); finalData.Add(t.Y);
                    }
                }
            }

            return finalData.ToArray();
        }
    }
}