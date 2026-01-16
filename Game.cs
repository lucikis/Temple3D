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
        private Texture _texArtifactIcon;
        private Texture _texCollectPrompt; // Imaginea cu "Press E"
        private Camera _camera;
        private SoundPlayer _collectSoundPlayer;

        private bool _showCollectPrompt = false;
        public enum GameState
        {
            Menu,
            Playing
        }

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

        // --- MENU UI ---
        private GameState _currentState = GameState.Menu;
        private Shader _uiShader;
        private Texture _texStartButton;
        private Texture _texExitButton;
        public Mesh _quadMesh; // Un pătrat simplu pentru butoane

        private Vector2 _btnStartPos;
        private Vector2 _btnExitPos;
        private Vector2 _btnSize = new Vector2(300, 100);

        // Definim geometria unui pătrat 2D (Quad)
        // Format: Pozitie(3), Normala(3), Textura(2) - pentru a fi compatibil cu Mesh-ul tau
        

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

            try { _texStartButton = new Texture("start_button.png"); } catch { _texStartButton = _textureCrate; }
            try { _texExitButton = new Texture("exit_button.png"); } catch { _texExitButton = _textureCrate; }

            try { _texArtifactIcon = new Texture("egg_artifact.png"); } catch { _texArtifactIcon = _textureCrate; }
            try { _texCollectPrompt = new Texture("collect_prompt.png"); } catch { _texCollectPrompt = _textureCrate; }

            // 2. Încărcăm Shader-ul de UI
            // NOTĂ: Asigură-te că ai creat fișierele ui.vert și ui.frag sau scrie codul direct aici.
            _uiShader = new Shader("ui.vert", "ui.frag");

            // 3. Creăm un Mesh simplu (un pătrat format din 2 triunghiuri)
            // Folosim același format ca Mesh.cs (8 floats: 3 pos, 3 norm, 2 tex)
            float[] quadVertices = {
            // Pos              // Norm (dummy)      // Tex (V inversat: 0 devine 1, 1 devine 0)
                -0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 0.0f, // Stânga Sus
                -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 1.0f, // Stânga Jos
                 0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // Dreapta Jos

                -0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 0.0f, // Stânga Sus
                 0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 1.0f, // Dreapta Jos
                 0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 0.0f  // Dreapta Sus
        };
            _quadMesh = new Mesh(quadVertices);

            // Poziționăm butoanele pe centrul ecranului
            _btnStartPos = new Vector2(Size.X / 2, Size.Y / 2 + 60);
            _btnExitPos = new Vector2(Size.X / 2, Size.Y / 2 - 60);

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

            //

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

        private void RenderHUD()
        {
            // 1. Setări pentru randare 2D peste 3D
            GL.Disable(EnableCap.DepthTest); // Dezactivăm testul de adâncime ca să desenăm peste tot
            GL.Enable(EnableCap.Blend);      // Activăm transparența (pentru PNG-uri cu fundal transparent)
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _uiShader.Use();

            // Matricea ortografică (aceeași ca la meniu)
            Matrix4 projectionUI = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1.0f, 1.0f);
            _uiShader.SetMatrix4("projection", projectionUI);

            // 2. Desenăm iconițele în funcție de scor
            float iconSize = 128.0f; // Dimensiunea iconiței (pixeli)
            float padding = 10.0f;  // Spațiu între iconițe
            float startX = 80.0f;   // Distanța față de marginea stângă
            float startY = 80.0f;   // Distanța față de marginea de sus

            for (int i = 0; i < _score; i++)
            {
                // Calculăm poziția pentru fiecare ou (le punem unul lângă altul)
                Vector2 position = new Vector2(startX + (i * (iconSize + padding)), startY);

                // Folosim funcția DrawTexture2D (fosta DrawButton redenumită sau refolosită)
                DrawTexture2D(position, new Vector2(iconSize, iconSize), _texArtifactIcon);
            }

            if (_showCollectPrompt)
            {
                float promptWidth = 250.0f;  // Ajustează mărimea după preferință
                float promptHeight = 166.6f;

                // Centrat orizontal, puțin mai jos de jumătatea ecranului
                float posX = (Size.X / 2.0f);
                float posY = (Size.Y / 2.0f) + 100.0f;

                Vector2 promptPos = new Vector2(posX, posY);
                Vector2 promptSize = new Vector2(promptWidth, promptHeight);

                DrawTexture2D(promptPos, promptSize, _texCollectPrompt);
            }

            // 3. Restaurăm setările pentru randarea 3D
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        // NOTĂ: Poți refolosi funcția 'DrawButton' de data trecută, 
        // dar este mai bine să o redenumești în 'DrawTexture2D' pentru claritate,
        // deoarece o folosim și pentru butoane și pentru HUD.
        private void DrawTexture2D(Vector2 position, Vector2 size, Texture texture)
        {
            texture.Use();

            Matrix4 model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(size.X, size.Y, 1.0f);
            model = model * Matrix4.CreateTranslation(position.X, position.Y, 0.0f);

            _uiShader.SetMatrix4("model", model);

            _quadMesh.Render();
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

            if (_currentState == GameState.Menu)
            {
                // Facem cursorul vizibil și liber
                CursorState = CursorState.Normal;

                var mouse = MouseState;

                // Verificăm dacă s-a dat click stânga
                if (mouse.IsButtonPressed(MouseButton.Left))
                {
                    // Verificăm coliziunea cu butonul START
                    if (IsMouseOverButton(mouse.Position, _btnStartPos, _btnSize))
                    {
                        _currentState = GameState.Playing;

                        // Când intrăm în joc, blocăm mouse-ul
                        CursorState = CursorState.Grabbed;
                    }

                    // Verificăm coliziunea cu butonul EXIT
                    if (IsMouseOverButton(mouse.Position, _btnExitPos, _btnSize))
                    {
                        Close(); // Închide aplicația
                    }
                }
                return; // NU rulăm logica jocului (mișcare, gravitație) cât timp suntem în meniu
            }

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
            _showCollectPrompt = false;

            for (int i = 0; i < _artifacts.Count; i++)
            {
                var art = _artifacts[i];
                if (art.IsActive)
                {
                    float distance = Vector3.Distance(_camera.Position, art.Position);

                    // Am mărit puțin distanța de interacțiune la 2.0f pentru a fi mai ușor
                    if (distance < 2.0f)
                    {
                        // 1. Activăm afișarea mesajului "Press E"
                        _showCollectPrompt = true;

                        // 2. Verificăm dacă jucătorul apasă E
                        if (input.IsKeyDown(Keys.E))
                        {
                            art.IsActive = false; // Îl ascundem (colectat)

                            // Redăm sunetul
                            if (_collectSoundPlayer != null)
                            {
                                _collectSoundPlayer.Play();
                            }

                            _score++;
                            Console.WriteLine($"Artefacte colectate: {_score}/3");

                            if (_score >= 3)
                            {
                                _portalOpen = true;
                                Console.WriteLine("Portalul s-a deschis!");
                                GL.ClearColor(0.2f, 0.1f, 0.1f, 1.0f);
                            }
                        }
                    }
                }
            }
        }

        private bool IsMouseOverButton(Vector2 mousePos, Vector2 btnCenter, Vector2 btnSize)
        {
            // Coordonatele mouse-ului în OpenTK pot avea originea diferită, dar de obicei sunt (0,0) sus-stânga.
            // Trebuie să verificăm dreptunghiul.
            float left = btnCenter.X - btnSize.X / 2;
            float right = btnCenter.X + btnSize.X / 2;
            float top = btnCenter.Y - btnSize.Y / 2;
            float bottom = btnCenter.Y + btnSize.Y / 2;

            return mousePos.X >= left && mousePos.X <= right &&
                   mousePos.Y >= top && mousePos.Y <= bottom;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_currentState == GameState.Playing)
            {
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

                GL.Enable(EnableCap.DepthTest);

                // --- 2. RANDARE HUD (Peste scena 3D) ---
                RenderHUD();
            }
            else if(_currentState == GameState.Menu)
            {
                GL.Disable(EnableCap.DepthTest);

                _uiShader.Use();

                // 2. Matricea de Proiecție Ortografică (2D)
                // Mapează pixelii (0,0) -> (Size.X, Size.Y)
                Matrix4 projectionUI = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1.0f, 1.0f);
                _uiShader.SetMatrix4("projection", projectionUI);

                // 3. Desenăm Butonul START
                DrawButton(_btnStartPos, _btnSize, _texStartButton);

                // 4. Desenăm Butonul EXIT
                DrawButton(_btnExitPos, _btnSize, _texExitButton);

                // Reactivăm Depth Test pentru frame-ul următor
                GL.Enable(EnableCap.DepthTest);
            }

            SwapBuffers();
        }

        private void DrawButton(Vector2 position, Vector2 size, Texture texture)
        {
            texture.Use();

            // Calculăm matricea Model pentru a muta și scala pătratul la dimensiunea butonului
            Matrix4 model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(size.X, size.Y, 1.0f); // Scalare la dimensiunea în pixeli
            model = model * Matrix4.CreateTranslation(position.X, position.Y, 0.0f); // Mutare la poziție

            _uiShader.SetMatrix4("model", model);

            _quadMesh.Render();
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

            // Recalculăm centrul butoanelor
            _btnStartPos = new Vector2(Size.X / 2, Size.Y / 2 - 60); // Atenție la direcția Y în Ortho
            _btnExitPos = new Vector2(Size.X / 2, Size.Y / 2 + 60);
        }
    }
}