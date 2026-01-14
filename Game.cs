using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;

namespace Temple3D
{
    public class Game : GameWindow
    {
        // --- RESURSE GRAFICE ---
        private Shader _shader;
        private Texture _texturePiatra;
        private Texture _textureIarba;
        private Texture _textureCrate;
        private Camera _camera;
        private SoundPlayer _collectSoundPlayer;

        // --- BUFFERING ---
        private int _vao;
        private int _vbo;

        // --- SCENA ȘI FIZICĂ ---
        // Lista care ține toate zidurile, podeaua și obstacolele
        private List<GameObject> _worldObjects = new List<GameObject>();

        // Variabile fizică jucător
        private float _verticalVelocity = 0.0f;
        private const float Gravity = -18.0f; // Gravitație (trage în jos)
        private const float JumpForce = 7.0f; // Puterea săriturii
        private bool _isGrounded = false;     // Verifică dacă stăm pe ceva

        private Mesh _cubeMesh;      // Mesh-ul standard pentru pereti/podea
        private Mesh _artifactMesh;  // Mesh-ul incarcat din fisier (ex: o cupa, o cheie)
        private Texture _artifactTexture;

        private List<GameObject> _artifacts = new List<GameObject>();
        private int _score = 0;
        private bool _portalOpen = false;

        // Dimensiuni jucător (hitbox)
        private const float PlayerRadius = 0.3f; // Cât de "gras" e jucătorul
        private const float PlayerHeight = 1.6f; // Înălțimea ochilor față de picioare

        private string[] _mapLayout =
        {
            "########",
            "#......#",
            "#......#",
            "#......#",
            "#..S...#",
            "#......#",
            "########"
        };

        // --- GEOMETRIE (CUB STANDARD - 36 Vârfuri) ---
        private readonly float[] _vertices =
        {
            // Positions          // Normals           // Texture Coords
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f, 1.0f,   0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,  0.0f, 1.0f
        };

        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f); // Fundal albastru închis
            GL.Enable(EnableCap.DepthTest);

            // 1. Inițializare Buffere
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Atribute: Pozitie (0), Normala (1), Textura (2)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            _cubeMesh = new Mesh(_vertices); // Foloseste array-ul _vertices existent in clasa ta

            // Încărcăm modelul extern
            // Exemplu: modelul se numeste "artefact.obj"
            float[] artifactData = ObjLoader.Load("scaled-model.obj");
            _artifactMesh = new Mesh(artifactData);
            _artifactTexture = new Texture("yellow.png");
            Console.WriteLine("Model incarcat cu succes!");

            // 2. Încărcare Shader și Texturi (Metoda simplă)
            _shader = new Shader("shader.vert", "shader.frag");

            try
            {
                // Încercăm să încărcăm texturile direct
                _texturePiatra = new Texture("stone_texture.png");
            }
            catch
            {
                Console.WriteLine("Eroare la incarcare stone.png");
            }

            try
            {
                _textureIarba = new Texture("grass.png");
            }
            catch
            {
                // Dacă nu avem iarbă, folosim piatră peste tot (fallback)
                _textureIarba = _texturePiatra;
            }

            try
            {
                // Încercăm să încărcăm texturile direct
                _textureCrate = new Texture("crate.png");
            }
            catch
            {
                Console.WriteLine("Eroare la incarcare crate.png");
            }

            try
            {
                // Asigură-te că numele fișierului este exact cel din proiect
                _collectSoundPlayer = new SoundPlayer("collect-sound.wav");
                _collectSoundPlayer.Load(); // Pre-încarcă sunetul pentru a fi gata instantaneu
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nu s-a putut încărca sunetul: {ex.Message}");
            }

            // 4. Cameră
            _camera = new Camera(new Vector3(0, 2.0f, 0), Size.X / (float)Size.Y);
            CursorState = CursorState.Grabbed;

            LoadMap();

            // Creare stratul 2 de ziduri
            for (int z = 9; z >= -15; z -= 6)
            {
                CreateWallLayer(18, z);
            }

            for (int x = 12; x >= -21; x -= 6)
            {
                CreateWallLayer(x, 15);
            }

            for (int x = 12; x >= -21; x -= 6)
            {
                CreateWallLayer(x, -21);
            }

            for (int z = 9; z >= -15; z -= 6)
            {
                CreateWallLayer(-24, z);
            }

            var crate1 = new GameObject(new Vector3(10, 0, 6), _cubeMesh, _textureCrate);
            crate1.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate1);

            var crate2 = new GameObject(new Vector3(11, 1.5f, 8), _cubeMesh, _textureCrate);
            crate2.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate2);

            var crate3 = new GameObject(new Vector3(9, 3.0f, 10), _cubeMesh, _textureCrate);
            crate3.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate3);

            var artifact1 = new GameObject(new Vector3(9, 5.0f, 10), _artifactMesh, _artifactTexture);
            artifact1.Scale = new Vector3(0.5f);
            _artifacts.Add(artifact1);

            var artifact2 = new GameObject(new Vector3(7, 1.5f, 7), _artifactMesh, _artifactTexture);
            artifact1.Scale = new Vector3(0.5f);
            _artifacts.Add(artifact2);

            var artifact3 = new GameObject(new Vector3(-2, 1, 6), _artifactMesh, _artifactTexture);
            artifact1.Scale = new Vector3(0.5f);
            _artifacts.Add(artifact3);
        }

        private void LoadMap()
        {
            float tileSize = 6.0f; // Dimensiunea unui pătrat

            // Calculăm offset-ul pentru a centra harta
            float offsetX = (_mapLayout[0].Length * tileSize) / 2.0f;
            float offsetZ = (_mapLayout.Length * tileSize) / 2.0f;

            for (int z = 0; z < _mapLayout.Length; z++)
            {
                string row = _mapLayout[z];
                for (int x = 0; x < row.Length; x++)
                {
                    char tileType = row[x];

                    // Calculăm poziția în lume
                    // X corespunde coloanei, Z corespunde rândului
                    float worldX = (x * tileSize) - offsetX;
                    float worldZ = (z * tileSize) - offsetZ;

                    Vector3 position = new Vector3(worldX, 0, worldZ);

                    // 1. Întotdeauna punem podea (ca să nu vedem "golul" sub artefacte sau jucător)
                    // Putem alterna textura pentru efectul de șah
                    Texture floorTex = _texturePiatra;
                    CreateFloorTile(position, tileSize, floorTex);

                    // 2. Verificăm ce obiect trebuie să fie deasupra podelei
                    switch (tileType)
                    {
                        case '#': // Zid
                            CreateWall(position, tileSize);
                            break;

                        case 'A': // Artefact
                            CreateArtifact(position);
                            break;

                        case 'S': // Start Player
                                  // Mutăm camera la această poziție (ridicată puțin pe Y)
                            _camera.Position = new Vector3(position.X, 2.0f, position.Z);
                            break;

                        case 'E': // Exit / Portal (Poți adăuga logica mai târziu)
                                  // CreatePortal(position);
                            break;
                    }
                }
            }
        }

        // Metode ajutătoare pentru a păstra codul curat

        private void CreateFloorTile(Vector3 pos, float size, Texture tex)
        {
            // Podeaua e la Y = -1.0f (sau mai jos, depinde de grosime)
            var floor = new GameObject(new Vector3(pos.X, -1.0f, pos.Z), _cubeMesh, tex);
            floor.Scale = new Vector3(size, 1.0f, size);
            _worldObjects.Add(floor);
        }

        private void CreateWall(Vector3 pos, float size)
        {
            // Zidul trebuie să fie mai înalt. Îl ridicăm pe Y ca să stea pe podea.
            // Un cub are înălțimea 1. Dacă îi dăm Scale Y=3, centrul e la 0, deci merge de la -1.5 la 1.5.
            // Ajustăm Y ca să pară că stă pe sol.
            var wall = new GameObject(new Vector3(pos.X, 1.5f, pos.Z), _cubeMesh, _texturePiatra);
            wall.Scale = new Vector3(size, 4.0f, size); // Zid înalt de 4 unități
            _worldObjects.Add(wall);
        }

        private void CreateWallLayer(int x, int z)
        {

            var new_wall = new GameObject(new Vector3(x, 5.5f, z), _cubeMesh, _texturePiatra);
            new_wall.Scale = new Vector3(6, 4.0f, 6); // Zid înalt de 4 unități
            _worldObjects.Add(new_wall);
        }

        private void CreateArtifact(Vector3 pos)
        {
            // Artefactul stă puțin deasupra solului
            var art = new GameObject(new Vector3(pos.X, 0.5f, pos.Z), _artifactMesh, _artifactTexture);

            // Ajustează scara în funcție de modelul tău
            art.Scale = new Vector3(0.7f);

            _artifacts.Add(art);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            float deltaTime = (float)e.Time;
            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape)) Close();

            // --- 1. LOGICA DE MIȘCARE ORIZONTALĂ (WASD) ---

            // Calculăm direcțiile FATA/DREAPTA ignorând Y 
            // (vrem sa mergem pe plan, nu sa zburam in sus cand privim in sus)
            Vector3 forward = Vector3.Normalize(new Vector3(_camera.GetViewMatrix().Column2.X, 0, _camera.GetViewMatrix().Column2.Z)) * -1;
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));

            Vector3 movement = Vector3.Zero;
            float speed = 5.0f * deltaTime;

            if (input.IsKeyDown(Keys.W)) movement += forward * speed;
            if (input.IsKeyDown(Keys.S)) movement -= forward * speed;
            if (input.IsKeyDown(Keys.A)) movement -= right * speed;
            if (input.IsKeyDown(Keys.D)) movement += right * speed;

            // Salvăm poziția veche pentru a putea reveni dacă ne lovim de zid
            Vector3 oldPos = _camera.Position;

            // Aplicăm mișcarea (doar pe X și Z deocamdată)
            _camera.Position += movement;

            // --- Coliziune cu pereții ---
            foreach (var obj in _worldObjects)
            {
                // CORECTIE AICI:
                // În loc să verificăm la 'PlayerHeight' (tălpi), verificăm puțin mai sus.
                // Scădem doar 1.0f în loc de 1.6f (PlayerHeight).
                // Asta ridică punctul de verificare la nivelul "brâului", evitând podeaua.

                float wallCheckOffset = 1.0f; // Verificăm mai sus de tălpi

                if (GameObject.CheckCollision(_camera.Position - Vector3.UnitY * wallCheckOffset, PlayerRadius, obj))
                {
                    // Dacă lovim un perete, anulăm mișcarea
                    _camera.Position = new Vector3(oldPos.X, _camera.Position.Y, oldPos.Z);
                    break;
                }
            }

            // --- 2. LOGICA DE GRAVITAȚIE ȘI SĂRITURI ---

            // Săritură (doar dacă suntem pe sol)
            if (input.IsKeyDown(Keys.Space) && _isGrounded)
            {
                _verticalVelocity = JumpForce;
                _isGrounded = false;
            }

            // Aplicăm gravitația constant
            _verticalVelocity += Gravity * deltaTime;

            // Aplicăm mișcarea pe Y
            _camera.Position += new Vector3(0, _verticalVelocity * deltaTime, 0);

            // --- Coliziune cu podeaua (Ground Check) ---
            _isGrounded = false;
            foreach (var obj in _worldObjects)
            {
                var bounds = obj.GetBounds();
                Vector3 feetPos = _camera.Position - Vector3.UnitY * PlayerHeight;

                // Suntem deasupra acestui obiect (în plan XZ)?
                bool inXZ = feetPos.X >= bounds.Min.X && feetPos.X <= bounds.Max.X &&
                            feetPos.Z >= bounds.Min.Z && feetPos.Z <= bounds.Max.Z;

                // Verificăm pe verticală: dacă picioarele au intrat puțin în obiect, dar capul e deasupra
                if (inXZ && feetPos.Y <= bounds.Max.Y && feetPos.Y >= bounds.Min.Y - 0.5f)
                {
                    // Dacă cădeam (viteză negativă), ne oprim
                    if (_verticalVelocity < 0)
                    {
                        _isGrounded = true;
                        _verticalVelocity = 0;
                        // Ne ridicăm exact pe suprafața obiectului
                        _camera.Position = new Vector3(_camera.Position.X, bounds.Max.Y + PlayerHeight, _camera.Position.Z);
                    }
                }
            }

            // Respawn dacă cădem de pe hartă
            if (_camera.Position.Y < -10)
            {
                _camera.Position = new Vector3(0, 5, 0);
                _verticalVelocity = 0;
            }

            // --- 3. ROTIRE CAMERĂ ---
            _camera.ProcessMouse(MouseState.Delta.X, MouseState.Delta.Y);

            foreach (var art in _artifacts)
            {
                {
                    if (art.IsActive)
                    {
                        // 1. Activăm textura
                        if (art.ObjectTexture != null) art.ObjectTexture.Use();

                        // 2. Calculăm matricea Model (Poziție, Rotație, Scară)
                        Matrix4 model = Matrix4.Identity;
                        model = model * Matrix4.CreateScale(art.Scale);
                        model = model * Matrix4.CreateTranslation(art.Position);

                        _shader.SetMatrix4("model", model);

                        // 3. AICI ESTE SCHIMBAREA CRITICĂ:
                        // NU folosi GL.DrawArrays(..., 0, 36);
                        // Folosește funcția Render din mesh-ul obiectului:
                        art.ObjectMesh.Render();
                    }
                }
            }

            // Logica Colectare Artefacte
            for (int i = 0; i < _artifacts.Count; i++)
            {
                var art = _artifacts[i];
                if (art.IsActive)
                {
                    float distance = Vector3.Distance(_camera.Position, art.Position);

                    if (distance < 1.5f) // Dacă suntem aproape de el
                    {
                        art.IsActive = false; // Îl ascundem

                        // --- AICI ADAUGI REDAREA SUNETULUI ---
                        if (_collectSoundPlayer != null)
                        {
                            // Play() rulează pe un thread separat (asincron), deci nu blochează jocul
                            _collectSoundPlayer.Play();
                        }
                        // -------------------------------------

                        _score++;
                        Console.WriteLine($"Artefacte colectate: {_score}/3");

                        if (_score >= 3)
                        {
                            _portalOpen = true;
                            Console.WriteLine("Portalul s-a deschis! Gaseste iesirea.");
                            GL.ClearColor(0.2f, 0.1f, 0.1f, 1.0f);
                        }
                    }
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();
            _shader.SetInt("uTexture", 0);

            // Uniforme pentru Lumină (lanterna jucătorului)
            _shader.SetVector3("viewPos", _camera.Position);
            _shader.SetVector3("lightPos", _camera.Position);
            _shader.SetVector3("lightColor", new Vector3(1.0f, 0.95f, 0.8f));

            // Matrici Cameră
            Matrix4 view = _camera.GetViewMatrix();
            Matrix4 projection = _camera.GetProjectionMatrix(Size.X / (float)Size.Y);
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);

            GL.BindVertexArray(_vao);

            foreach (var obj in _worldObjects)
            {
                DrawObject(obj);
            }

            // Randare Artefacte
            foreach (var art in _artifacts)
            {
                if (art.IsActive)
                {
                    DrawObject(art);
                }
            }

            SwapBuffers();
        }

        private void DrawObject(GameObject obj)
        {
            if (obj.ObjectTexture != null) obj.ObjectTexture.Use();

            Matrix4 model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(obj.Scale);
            // Putem adauga rotatia aici daca o stocam in GameObject
            // model = model * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(obj.Rotation.Y));
            model = model * Matrix4.CreateTranslation(obj.Position);

            _shader.SetMatrix4("model", model);

            // AICI ESTE SCHIMBAREA CHEIE: Apelam Render pe Mesh-ul specific obiectului
            obj.ObjectMesh.Render();
        }

        protected override void OnUnload()
        {
            // Curățenie
            _cubeMesh.Dispose();
            if (_artifactMesh != _cubeMesh) _artifactMesh.Dispose();
            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }
}