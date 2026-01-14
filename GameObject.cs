using OpenTK.Mathematics;

namespace Temple3D
{
    public class GameObject
    {
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Rotation { get; set; } = Vector3.Zero;

        public Texture ObjectTexture { get; set; }
        public Mesh ObjectMesh { get; set; } // Referință către geometria obiectului

        public bool IsActive { get; set; } = true; // Pentru a ascunde artefactele colectate

        public GameObject(Vector3 pos, Mesh mesh, Texture tex)
        {
            Position = pos;
            ObjectMesh = mesh;
            ObjectTexture = tex;
        }

        // Returnează limite aproximative.
        // NOTA: Pentru modele complexe importate, Scale trebuie ajustat manual
        // pentru ca coliziunea să aibă sens.
        public (Vector3 Min, Vector3 Max) GetBounds()
        {
            Vector3 halfSize = new Vector3(0.5f) * Scale;
            return (Position - halfSize, Position + halfSize);
        }

        // Verifică coliziunea simplă (AABB vs Punct/Raza)
        public static bool CheckCollision(Vector3 point, float radius, GameObject obj)
        {
            if (!obj.IsActive) return false; // Nu ne lovim de obiecte invizibile

            var bounds = obj.GetBounds();
            bool collisionX = point.X + radius > bounds.Min.X && point.X - radius < bounds.Max.X;
            bool collisionY = point.Y + radius > bounds.Min.Y && point.Y - radius < bounds.Max.Y;
            bool collisionZ = point.Z + radius > bounds.Min.Z && point.Z - radius < bounds.Max.Z;

            return collisionX && collisionY && collisionZ;
        }
    }
}