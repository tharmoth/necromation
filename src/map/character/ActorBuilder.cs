using System.Linq;
using Godot;
using Necromation;
using Necromation.bridges;
using Necromation.map;
using Necromation.map.character;

public partial class Actor
{
    /**************************************************************************
     * Constructor                                                            *
     **************************************************************************/
    private Actor(IInputHandler inputHandler)
    {
        _inputHandler = inputHandler;
    }
    
    public class ActorBuilder
    {
        private Actor _actor;

        public static ActorBuilder Create()
        {
            return new ActorBuilder();
        }

        public ActorBuilder AsPlayer()
        {
            var startPosition = Globals.MapScene
                .Map
                .Provinces
                .First(province => province.Owner == "Player")
                .Position;
            _actor = new Actor(new PlayerInputHandler(startPosition));
            _actor.Position = startPosition;

            var sprite = new Sprite2D() { Texture = Database.Instance.GetTexture("Player-south") };
            sprite.ScaleToSize(Vector2.One * 32.0f);
            _actor.AddComponent(sprite);
            _actor.AddComponent(new StatsComponent());
            _actor.AddComponent(new VelocityComponent { Parent = _actor });

            var army = new ArmyComponent()
            {
                OnBattleLost = () =>
                {
                    _actor.Position = startPosition;
                }
            };
            _actor.AddComponent(army);
            
            return this;
        }

        public ActorBuilder AsEnemy()
        {
            _actor = new Actor(new EnemyInputHandler());

            var sprite = new Sprite2D { Texture = Database.Instance.GetTexture("Rabble") };
            sprite.ScaleToSize(Vector2.One * 32.0f);
            
            _actor.AddComponent(sprite);
            var stats = new StatsComponent {Team = "Enemy"};
            stats.WalkSpeed = 50;
            _actor.AddComponent(stats);
            _actor.AddComponent(new VelocityComponent { Parent = _actor });

            return this;
        }

        public ActorBuilder AsMapLocation(Province province)
        {
            _actor = new Actor(new NullInputHandler());
            _actor.Position = province.Position;

            var texture = province.Owner == "Player" ? "Barracks" : "House";
            var sprite = new Sprite2D { Texture = Database.Instance.GetTexture(texture) };
            sprite.ScaleToSize(Vector2.One * 32.0f);
            _actor.AddComponent(sprite);

            InteractComponent interact;
            if (province.Owner == "Player")
            {
                interact = new InteractComponent {Parent = _actor, OnInteract = () => SceneManager.ChangeToScene(SceneManager.SceneEnum.Factory)};
            }
            else
            {
                interact = new InteractComponent {Parent = _actor, OnInteract = () =>
                    {
                        MapBattleBridge.Battle(_actor);
                    }
                };
            }
            _actor.AddComponent(interact);
            
            var army = new ArmyComponent()
            {
                OnBattleLost = () =>
                {
                    province.Owner = "Player";
                    _actor.QueueFree();
                }
            };
            var commander = new Commander(province.Owner);
            commander.Units.Insert("Rabble", 10);
            army.Commanders.Add(commander);
            _actor.AddComponent(army);
            
            var area = new Area2D();
            var shape = new CollisionShape2D
            {
                Shape = new RectangleShape2D { Size = new Vector2(40, 20) }
            };
            area.AddChild(shape);
            _actor.AddComponent(area);

            area.MouseEntered += () =>
            {
                InteractComponent.MouseOverQueue.Add(interact);
            };
            area.MouseExited += () =>
            {
                InteractComponent.MouseOverQueue.Remove(interact);
            };

            return this;
        }

        public Actor Build()
        {
            return _actor;
        }
    }
}