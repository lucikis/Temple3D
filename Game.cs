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
        private Shader _shader;
        private Texture _texturePiatra;
        private Texture _textureIarba;
        private Texture _textureCrate;
        private Texture _texArtifactIcon;
        private Texture _texCollectPrompt;
        private Texture _texObjective;
        private Texture _texThankYou;
        private GameObject _portalObject;
        private bool _gameFinished = false;

        private Camera _camera;
        private SoundPlayer _collectSoundPlayer;

        private bool _showCollectPrompt = false;
        public enum GameState
        {
            Menu,
            Playing
        }

        private int _vao;
        private int _vbo;

        private List<GameObject> _worldObjects = new List<GameObject>();

        private float _verticalVelocity = 0.0f;
        private const float Gravity = -18.0f;
        private const float JumpForce = 7.0f;
        private bool _isGrounded = false;

        private Mesh _cubeMesh;
        private Mesh _artifactMesh;
        private Texture _artifactTexture;
        private Mesh _portalMesh;
        private Texture _portalTexture;

        private List<GameObject> _artifacts = new List<GameObject>();
        private int _score = 0;
        private bool _portalOpen = false;

        private const float PlayerRadius = 0.3f;
        private const float PlayerHeight = 1.6f;

        private GameState _currentState = GameState.Menu;
        private Shader _uiShader;
        private Texture _texTitle;
        private Texture _texStartButton;
        private Texture _texExitButton;
        public Mesh _quadMesh;

        private Vector2 _titlePos;
        private Vector2 _btnStartPos;
        private Vector2 _btnExitPos;
        private Vector2 _objectivePos;
        private Vector2 _btnSize = new Vector2(300, 100);
        private Vector2 _titleSize = new Vector2(800, 379.1f);
        private Vector2 _objectiveSize = new Vector2(500, 90);

        private readonly float[] _vertices =
        {
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

            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            _cubeMesh = new Mesh(_vertices);

            try { _texTitle = new Texture("title.png"); } catch { _texTitle = _textureCrate; }
            try { _texStartButton = new Texture("start_button.png"); } catch { _texStartButton = _textureCrate; }
            try { _texExitButton = new Texture("exit_button.png"); } catch { _texExitButton = _textureCrate; }
            try { _texArtifactIcon = new Texture("egg_artifact.png"); } catch { _texArtifactIcon = _textureCrate; }
            try { _texCollectPrompt = new Texture("collect_prompt.png"); } catch { _texCollectPrompt = _textureCrate; }
            try { _texThankYou = new Texture("thank_you.png"); } catch { _texThankYou = _textureCrate; }
            try { _texObjective = new Texture("objective.png"); } catch { _texObjective = _textureCrate; }

            _uiShader = new Shader("ui.vert", "ui.frag");

            float[] quadVertices = {
                -0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 0.0f,
                -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 1.0f,
                 0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 1.0f,

                -0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   0.0f, 0.0f,
                 0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 1.0f,
                 0.5f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f,   1.0f, 0.0f
            };
            _quadMesh = new Mesh(quadVertices);

            _titlePos = new Vector2(Size.X / 2, Size.Y / 2 - 400);
            _btnStartPos = new Vector2(Size.X / 2, Size.Y / 2 + 60);
            _btnExitPos = new Vector2(Size.X / 2, Size.Y / 2 - 60);
            _objectivePos = new Vector2(Size.X / 2, Size.Y / 2  + 250);

            float[] artifactData = ObjLoader.Load("scaled-model.obj");
            _artifactMesh = new Mesh(artifactData);
            _artifactTexture = new Texture("yellow.png");

            float[] portalData = ObjLoader.Load("scaled_portal.obj");
            _portalMesh = new Mesh(portalData);
            _portalTexture = new Texture("purple.png");

            _shader = new Shader("shader.vert", "shader.frag");

            try { _texturePiatra = new Texture("stone_texture.png"); } catch { }
            try { _textureIarba = new Texture("grass.png"); } catch { _textureIarba = _texturePiatra; }
            try { _textureCrate = new Texture("crate.png"); } catch { }
            try { _collectSoundPlayer = new SoundPlayer("collect-sound.wav"); _collectSoundPlayer.Load(); } catch { }

            _camera = new Camera(new Vector3(0, 2.0f, 0), Size.X / (float)Size.Y);
            CursorState = CursorState.Grabbed;

            for (int z = 9; z >= -15; z -= 6) CreateWallLayer(18, 7, z);
            for (int x = 12; x >= -21; x -= 6) CreateWallLayer(x, 7, 15);
            for (int x = 12; x >= -21; x -= 6) CreateWallLayer(x, 7, -21);
            for (int z = 9; z >= -15; z -= 6) CreateWallLayer(-24, 7, z);

            for (int z = 9; z >= -15; z -= 6) CreateWallLayer(18, 3, z);
            for (int x = 12; x >= -21; x -= 6) CreateWallLayer(x, 3, 15);
            for (int x = 12; x >= -21; x -= 6) CreateWallLayer(x, 3, -21);
            for (int z = 9; z >= -15; z -= 6) CreateWallLayer(-24, 3, z);

            for (int x = -24; x <= 18; x += 6)
            {
                for (int z = -15; z <= 9; z += 6)
                {
                    var tavan = new GameObject(new Vector3(x, 11, z), _cubeMesh, _texturePiatra);
                    tavan.Scale = new Vector3(6, 4, 6);
                    _worldObjects.Add(tavan);

                    var podea = new GameObject(new Vector3(x, -1, z), _cubeMesh, _texturePiatra);
                    podea.Scale = new Vector3(6, 4, 6);
                    _worldObjects.Add(podea);
                }
            }

            var crate1 = new GameObject(new Vector3(10, 1.5f, 6), _cubeMesh, _textureCrate);
            crate1.Scale = new Vector3(1, 1, 1);
            crate1.Rotation = new Vector3(0, 45f, 0f);
            _worldObjects.Add(crate1);

            var crate2 = new GameObject(new Vector3(11, 3, 8), _cubeMesh, _textureCrate);
            crate2.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate2);

            var crate3 = new GameObject(new Vector3(9, 4.5f, 10), _cubeMesh, _textureCrate);
            crate3.Scale = new Vector3(1, 1, 1);
            crate3.Rotation = new Vector3(0, 45f, 0f);
            _worldObjects.Add(crate3);

            var crate4 = new GameObject(new Vector3(6, 6, 10), _cubeMesh, _textureCrate);
            crate4.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate4);

            var artifact1 = new GameObject(new Vector3(6, 7.5f, 10), _artifactMesh, _artifactTexture);
            artifact1.Scale = new Vector3(0.5f);
            _artifacts.Add(artifact1);

            _portalObject = new GameObject(new Vector3(14.95f, 2.6f, -3), _portalMesh, _portalTexture);
            _portalObject.Scale = new Vector3(0.5f);
            _portalObject.IsActive = false;

            var crate5 = new GameObject(new Vector3(6, 4.5f, -16), _cubeMesh, _textureCrate);
            crate5.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate5);

            var crate6 = new GameObject(new Vector3(9, 3, -16), _cubeMesh, _textureCrate);
            crate6.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate6);

            var crate7 = new GameObject(new Vector3(12, 1.5f, -16), _cubeMesh, _textureCrate);
            crate7.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate7);

            var artifact2 = new GameObject(new Vector3(6, 6, -16), _artifactMesh, _artifactTexture);
            artifact2.Scale = new Vector3(0.5f);
            _artifacts.Add(artifact2);

            var crate8 = new GameObject(new Vector3(-12, 4.5f, -16), _cubeMesh, _textureCrate);
            crate8.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate8);

            var crate9 = new GameObject(new Vector3(-9, 3, -16), _cubeMesh, _textureCrate);
            crate9.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate9);

            var crate10 = new GameObject(new Vector3(-6, 1.5f, -16), _cubeMesh, _textureCrate);
            crate10.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate10);

            var crate11 = new GameObject(new Vector3(-12, 4.5f, 8), _cubeMesh, _textureCrate);
            crate11.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate11);

            var crate12 = new GameObject(new Vector3(-9, 3, 8), _cubeMesh, _textureCrate);
            crate12.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate12);

            var crate13 = new GameObject(new Vector3(-6, 1.5f, 8), _cubeMesh, _textureCrate);
            crate13.Scale = new Vector3(1, 1, 1);
            _worldObjects.Add(crate13);

            for (int z = -15; z <= 9; z += 6)
            {
                var platforma = new GameObject(new Vector3(-18, 5, z), _cubeMesh, _texturePiatra);
                platforma.Scale = new Vector3(6, 0.5f, 6);
                _worldObjects.Add(platforma);
            }

            for (int z = -14; z <= 6; z += 4)
            {
                var wall = new GameObject(new Vector3(-18, 7, z), _cubeMesh, _texturePiatra);
                wall.Scale = new Vector3(6, 4, 0.3f);
                _worldObjects.Add(wall);
            }

            var artifact3 = new GameObject(new Vector3(-18, 6, 9), _artifactMesh, _artifactTexture);
            artifact3.Scale = new Vector3(0.5f);
            _artifacts.Add(artifact3);
        }

        private void RenderHUD()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _uiShader.Use();

            Matrix4 projectionUI = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1.0f, 1.0f);
            _uiShader.SetMatrix4("projection", projectionUI);

            if (_gameFinished)
            {
                DrawTexture2D(new Vector2(Size.X / 2, Size.Y / 2), new Vector2(1000, 591.7f), _texThankYou);

                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.DepthTest);
                return;
            }

            for (int i = 0; i < _score; i++)
            {
                Vector2 position = new Vector2(80 + (i * 138), 80);
                DrawTexture2D(position, new Vector2(128, 128), _texArtifactIcon);
            }

            if (_showCollectPrompt)
            {
                float promptWidth = 300.0f;
                float promptHeight = 200.0f;
                float posX = (Size.X / 2.0f);
                float posY = (Size.Y / 2.0f) + 100.0f;

                DrawTexture2D(new Vector2(posX, posY), new Vector2(promptWidth, promptHeight), _texCollectPrompt);
            }

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        private void DrawTexture2D(Vector2 position, Vector2 size, Texture texture)
        {
            if (texture == null) return;
            texture.Use();

            Matrix4 model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(size.X, size.Y, 1.0f);
            model = model * Matrix4.CreateTranslation(position.X, position.Y, 0.0f);

            _uiShader.SetMatrix4("model", model);
            _quadMesh.Render();
        }

        private void CreateWallLayer(float x, float y, float z)
        {
            var new_wall = new GameObject(new Vector3(x, y, z), _cubeMesh, _texturePiatra);
            new_wall.Scale = new Vector3(6, 4.0f, 6);
            _worldObjects.Add(new_wall);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (_currentState == GameState.Menu)
            {
                CursorState = CursorState.Normal;
                var mouse = MouseState;

                if (mouse.IsButtonPressed(MouseButton.Left))
                {
                    if (IsMouseOverButton(mouse.Position, _btnStartPos, _btnSize))
                    {
                        _currentState = GameState.Playing;
                        CursorState = CursorState.Grabbed;
                    }
                    if (IsMouseOverButton(mouse.Position, _btnExitPos, _btnSize))
                    {
                        Close();
                    }
                }
                return;
            }

            if (_gameFinished)
            {
                if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
                return;
            }

            float deltaTime = (float)e.Time;
            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape)) Close();

            Vector3 forward = Vector3.Normalize(new Vector3(_camera.GetViewMatrix().Column2.X, 0, _camera.GetViewMatrix().Column2.Z)) * -1;
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));

            Vector3 movement = Vector3.Zero;
            float speed = 5.0f * deltaTime;

            if (input.IsKeyDown(Keys.W)) movement += forward * speed;
            if (input.IsKeyDown(Keys.S)) movement -= forward * speed;
            if (input.IsKeyDown(Keys.A)) movement -= right * speed;
            if (input.IsKeyDown(Keys.D)) movement += right * speed;

            Vector3 oldPos = _camera.Position;
            _camera.Position += movement;

            foreach (var obj in _worldObjects)
            {
                float wallCheckOffset = 1.0f;
                if (GameObject.CheckCollision(_camera.Position - Vector3.UnitY * wallCheckOffset, PlayerRadius, obj))
                {
                    _camera.Position = new Vector3(oldPos.X, _camera.Position.Y, oldPos.Z);
                    break;
                }
            }

            if (input.IsKeyDown(Keys.Space) && _isGrounded)
            {
                _verticalVelocity = JumpForce;
                _isGrounded = false;
            }

            _verticalVelocity += Gravity * deltaTime;
            _camera.Position += new Vector3(0, _verticalVelocity * deltaTime, 0);

            _isGrounded = false;
            foreach (var obj in _worldObjects)
            {
                var bounds = obj.GetBounds();
                Vector3 feetPos = _camera.Position - Vector3.UnitY * PlayerHeight;

                bool inXZ = feetPos.X >= bounds.Min.X && feetPos.X <= bounds.Max.X &&
                            feetPos.Z >= bounds.Min.Z && feetPos.Z <= bounds.Max.Z;

                if (inXZ && feetPos.Y <= bounds.Max.Y && feetPos.Y >= bounds.Min.Y - 0.5f)
                {
                    if (_verticalVelocity < 0)
                    {
                        _isGrounded = true;
                        _verticalVelocity = 0;
                        _camera.Position = new Vector3(_camera.Position.X, bounds.Max.Y + PlayerHeight, _camera.Position.Z);
                    }
                }
            }

            if (_camera.Position.Y < -10)
            {
                _camera.Position = new Vector3(0, 5, 0);
                _verticalVelocity = 0;
            }

            _camera.ProcessMouse(MouseState.Delta.X, MouseState.Delta.Y);

            if (_score >= 3 && !_portalObject.IsActive)
            {
                _portalObject.IsActive = true;
                _portalOpen = true;
                Console.WriteLine("Portalul s-a deschis!");
                GL.ClearColor(0.2f, 0.1f, 0.1f, 1.0f);
            }

            if (_portalObject.IsActive)
            {
                float distToPortal = Vector3.Distance(_camera.Position, _portalObject.Position);
                if (distToPortal < 2.0f)
                {
                    _gameFinished = true;
                    CursorState = CursorState.Normal;
                }
            }

            _showCollectPrompt = false;

            for (int i = 0; i < _artifacts.Count; i++)
            {
                var art = _artifacts[i];
                if (art.IsActive)
                {
                    float distance = Vector3.Distance(_camera.Position, art.Position);

                    if (distance < 2.0f)
                    {
                        _showCollectPrompt = true;

                        if (input.IsKeyDown(Keys.E))
                        {
                            art.IsActive = false;
                            if (_collectSoundPlayer != null) _collectSoundPlayer.Play();

                            _score++;
                            Console.WriteLine($"Artefacte colectate: {_score}/3");
                        }
                    }
                }
            }
        }

        private bool IsMouseOverButton(Vector2 mousePos, Vector2 btnCenter, Vector2 btnSize)
        {
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

                _shader.SetVector3("viewPos", _camera.Position);
                _shader.SetVector3("lightPos", _camera.Position);
                _shader.SetVector3("lightColor", new Vector3(1.0f, 0.95f, 0.8f));

                Matrix4 view = _camera.GetViewMatrix();
                Matrix4 projection = _camera.GetProjectionMatrix(Size.X / (float)Size.Y);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);

                GL.BindVertexArray(_vao);

                foreach (var obj in _worldObjects)
                {
                    DrawObject(obj);
                }

                foreach (var art in _artifacts)
                {
                    if (art.IsActive)
                    {
                        DrawObject(art);
                    }
                }

                if (_portalObject.IsActive)
                {
                    DrawObject(_portalObject);
                }

                GL.Enable(EnableCap.DepthTest);

                RenderHUD();
            }
            else if (_currentState == GameState.Menu)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                _uiShader.Use();

                Matrix4 projectionUI = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1.0f, 1.0f);
                _uiShader.SetMatrix4("projection", projectionUI);

                DrawButton(_titlePos, _titleSize, _texTitle);
                DrawButton(_btnStartPos, _btnSize, _texStartButton);
                DrawButton(_btnExitPos, _btnSize, _texExitButton);
                DrawButton(_objectivePos, _objectiveSize, _texObjective);

                GL.Enable(EnableCap.DepthTest);
            }

            SwapBuffers();
        }

        private void DrawButton(Vector2 position, Vector2 size, Texture texture)
        {
            if (texture == null) return;
            texture.Use();

            Matrix4 model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(size.X, size.Y, 1.0f);
            model = model * Matrix4.CreateTranslation(position.X, position.Y, 0.0f);

            _uiShader.SetMatrix4("model", model);
            _quadMesh.Render();
        }

        private void DrawObject(GameObject obj)
        {
            if (obj.ObjectTexture != null) obj.ObjectTexture.Use();

            Matrix4 model = Matrix4.Identity;
            model = model * Matrix4.CreateScale(obj.Scale);

            model = model * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(obj.Rotation.X));
            model = model * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(obj.Rotation.Y));
            model = model * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(obj.Rotation.Z));

            model = model * Matrix4.CreateTranslation(obj.Position);

            _shader.SetMatrix4("model", model);
            obj.ObjectMesh.Render();
        }

        protected override void OnUnload()
        {
            _cubeMesh.Dispose();
            if (_artifactMesh != _cubeMesh) _artifactMesh.Dispose();
            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);

            _btnStartPos = new Vector2(Size.X / 2, Size.Y / 2 - 60);
            _btnExitPos = new Vector2(Size.X / 2, Size.Y / 2 + 60);
        }
    }
}