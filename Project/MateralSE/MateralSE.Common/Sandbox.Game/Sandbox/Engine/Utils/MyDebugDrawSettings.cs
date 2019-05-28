namespace Sandbox.Engine.Utils
{
    using System;
    using VRage.Game;
    using VRage.Game.Voxels;
    using VRageRender;
    using VRageRender.Utils;

    public static class MyDebugDrawSettings
    {
        private static bool m_enableDebugDraw = false;
        public static bool DEBUG_DRAW_ENTITY_IDS = false;
        public static bool DEBUG_DRAW_ENTITY_IDS_ONLY_ROOT = true;
        public static bool DEBUG_DRAW_BLOCK_NAMES = false;
        public static bool DEBUG_DRAW_AUDIO = false;
        public static bool DEBUG_DRAW_PHYSICS = false;
        public static bool DEBUG_DRAW_MOUNT_POINTS = false;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AXIS_HELPERS = false;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AUTOGENERATE = false;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AXIS0 = true;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AXIS1 = true;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AXIS2 = true;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AXIS3 = true;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AXIS4 = true;
        public static bool DEBUG_DRAW_MOUNT_POINTS_AXIS5 = true;
        public static bool DEBUG_DRAW_MOUNT_POINTS_ALL = false;
        public static bool DEBUG_DRAW_MODEL_DUMMIES = false;
        public static bool DEBUG_DRAW_GAME_PRUNNING = false;
        public static bool DEBUG_DRAW_RADIO_BROADCASTERS = false;
        public static bool DEBUG_DRAW_STOCKPILE_QUANTITIES = false;
        public static bool DEBUG_DRAW_SUIT_BATTERY_CAPACITY = false;
        public static bool DEBUG_DRAW_CHARACTER_BONES = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_ANKLE_FINALPOS = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_SETTINGS = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_RAYCASTLINE = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_BONES = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_RAYCASTHITS = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_ANKLE_DESIREDPOSITION = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_CLOSESTSUPPORTPOSITION = false;
        public static bool DEBUG_DRAW_CHARACTER_IK_IKSOLVERS = false;
        public static MyCharacterMovementEnum DEBUG_DRAW_CHARACTER_IK_MOVEMENT_STATE = MyCharacterMovementEnum.Standing;
        public static bool DEBUG_DRAW_CHARACTER_RAGDOLL_ORIGINAL_RIG = false;
        public static bool DEBUG_DRAW_CHARACTER_RAGDOLL_POSE = false;
        public static bool DEBUG_DRAW_CHARACTER_RAGDOLL_COMPUTED_BONES = false;
        public static bool DEBUG_DRAW_CHARACTER_RAGDOLL_HIPPOSITIONS = false;
        public static bool DEBUG_DRAW_NEUTRAL_SHIPS = false;
        public static bool DEBUG_DRAW_DRONES = false;
        public static bool DEBUG_DRAW_DISPLACED_BONES = false;
        public static bool DEBUG_DRAW_CUBE_BLOCK_AABBS = false;
        public static bool DEBUG_DRAW_CHARACTER_MISC = false;
        public static bool DEBUG_DRAW_EVENTS = false;
        public static bool DEBUG_DRAW_RESOURCE_RECEIVERS = false;
        public static bool DEBUG_DRAW_COCKPIT = false;
        public static bool DEBUG_DRAW_CONVEYORS = false;
        public static bool DEBUG_DRAW_CUBES = false;
        public static bool DEBUG_DRAW_TRIANGLE_PHYSICS = false;
        public static bool DEBUG_DRAW_GRID_GROUPS_PHYSICAL = false;
        public static bool DEBUG_DRAW_GRID_GROUPS_LOGICAL = false;
        public static bool DEBUG_DRAW_STRUCTURAL_INTEGRITY = false;
        public static bool DEBUG_DRAW_VOLUMETRIC_EXPLOSION_COLORING = false;
        public static bool DEBUG_DRAW_CONVEYORS_LINE_IDS = false;
        public static bool DEBUG_DRAW_CONVEYORS_LINE_CAPSULES = false;
        public static bool DEBUG_DRAW_REMOVE_CUBE_COORDS = false;
        public static bool DEBUG_DRAW_GRID_COUNTER = false;
        public static bool DEBUG_DRAW_GRID_NAMES = false;
        public static bool DEBUG_DRAW_GRID_CONTROL = false;
        public static bool DEBUG_DRAW_GRID_TERMINAL_SYSTEMS = false;
        public static bool DEBUG_DRAW_CONNECTORS_AND_MERGE_BLOCKS = false;
        public static bool DEBUG_DRAW_COPY_PASTE = false;
        public static bool DEBUG_DRAW_GRID_ORIGINS = false;
        public static bool DEBUG_DRAW_GRID_AABB = false;
        public static bool DEBUG_DRAW_THRUSTER_DAMAGE = false;
        public static bool DEBUG_DRAW_BLOCK_GROUPS = false;
        public static bool DEBUG_DRAW_ROTORS = false;
        public static bool DEBUG_DRAW_GYROS = false;
        public static bool DEBUG_DRAW_VOXEL_GEOMETRY_CELL = false;
        public static bool DEBUG_DRAW_VOXEL_MAP_AABB = false;
        public static bool DEBUG_DRAW_RESPAWN_SHIP_COUNTERS = false;
        public static bool DEBUG_DRAW_EXPLOSION_HAVOK_RAYCASTS = false;
        public static bool DEBUG_DRAW_EXPLOSION_DDA_RAYCASTS = false;
        public static bool DEBUG_DRAW_CONTROLLED_ENTITIES = false;
        public static bool DEBUG_DRAW_PHYSICS_CLUSTERS = false;
        public static bool DEBUG_DRAW_GRID_DIRTY_BLOCKS = false;
        public static bool DEBUG_DRAW_MERGED_GRIDS = false;
        public static bool DEBUG_DRAW_VOXEL_PHYSICS_PREDICTION = false;
        public static bool DEBUG_DRAW_VOXEL_MAP_BOUNDING_BOX = false;
        public static bool DEBUG_DRAW_MODEL_INFO = false;
        public static bool DEBUG_DRAW_FRACTURED_PIECES = false;
        public static bool DEBUG_DRAW_ENVIRONMENT_ITEMS = false;
        public static bool DEBUG_DRAW_SMALL_TO_LARGE_BLOCK_GROUPS = false;
        public static bool DEBUG_DRAW_DYNAMIC_PHYSICAL_GROUPS = false;
        public static bool DEBUG_DRAW_ROPES = false;
        public static bool DEBUG_DRAW_OXYGEN = false;
        public static bool DEBUG_DRAW_ANIMALS = false;
        public static bool DEBUG_DRAW_VOICE_CHAT = false;
        public static bool DEBUG_DRAW_FLORA = false;
        public static bool DEBUG_DRAW_FLORA_SPAWN_INFO = false;
        public static bool DEBUG_DRAW_FLORA_REGROW_INFO = false;
        public static bool DEBUG_DRAW_FLORA_BOXES = false;
        public static bool DEBUG_DRAW_FLORA_SPAWNED_ITEMS = false;
        public static bool DEBUG_DRAW_ENTITY_COMPONENTS = false;
        public static bool DEBUG_DRAW_GRIDS_DECAY = false;
        public static bool DEBUG_DRAW_ENTITY_STATISTICS = false;
        public static bool DEBUG_DRAW_GRID_STATISTICS = false;
        public static bool DEBUG_DRAW_GRID_HIERARCHY = false;
        public static MyWEMDebugDrawMode DEBUG_DRAW_NAVMESHES = MyWEMDebugDrawMode.NONE;
        internal static MyVoxelDebugDrawMode DEBUG_DRAW_VOXELS_MODE = MyVoxelDebugDrawMode.None;
        public static bool DEBUG_DRAW_INTERPOLATION = false;
        public static bool DEBUG_DRAW_MISCELLANEOUS = false;
        public static bool DEBUG_DRAW_METEORITS_DIRECTIONS = false;
        public static bool BREAKABLE_SHAPE_CHILD_COUNT = false;
        public static bool BREAKABLE_SHAPE_CONNECTIONS = false;
        public static bool DEBUG_DRAW_BOTS = false;
        public static bool DEBUG_DRAW_BOT_AIMING = false;
        public static bool DEBUG_DRAW_BOT_STEERING = false;
        public static bool DEBUG_DRAW_BOT_NAVIGATION = false;
        public static bool DEBUG_DRAW_SHOW_DAMAGE = false;
        public static bool DEBUG_DRAW_CHARACTER_RAGDOLL_BONES_ORIGINAL_RIG = false;
        public static bool DEBUG_DRAW_CHARACTER_RAGDOLL_BONES_DESIRED = false;
        public static bool DEBUG_DRAW_BLOCK_INTEGRITY = false;
        public static bool DEBUG_DRAW_FIXED_BLOCK_QUERIES = false;
        public static bool DEBUG_DRAW_DRILLS = false;
        public static bool DEBUG_DRAW_PHYSICS_SHAPES = true;
        public static bool DEBUG_DRAW_PHYSICS_SIMULATION_ISLANDS = false;
        public static bool DEBUG_DRAW_PHYSICS_MOTION_TYPES = false;
        public static bool DEBUG_DRAW_INERTIA_TENSORS = false;
        public static bool DEBUG_DRAW_SORTED_JOBS = false;
        public static bool DEBUG_DRAW_PHYSICS_FORCES = false;
        public static bool DEBUG_DRAW_SUSPENSION_POWER = false;
        public static bool DEBUG_DRAW_CONSTRAINTS = false;
        public static bool DEBUG_DRAW_FRICTION = false;
        public static bool DEBUG_DRAW_UPDATE_TRIGGER = false;
        public static bool DEBUG_DRAW_REQUEST_SHAPE_BLOCKING = false;
        public static bool DEBUG_DRAW_FAUNA_COMPONENT = false;
        public static bool DEBUG_DRAW_WHEEL_PHYSICS = false;
        public static bool DEBUG_DRAW_WHEEL_SYSTEMS = false;
        public static float DEBUG_DRAW_MODEL_DUMMIES_DISTANCE = 0f;
        public static bool DEBUG_DRAW_PLANET_SECTORS = false;
        public static bool DEBUG_DRAW_PARTICLES = false;
        public static bool DEBUG_DRAW_REGROWTH_ACTIVE_MODULES = false;
        public static bool DEBUG_DRAW_REGROWTH_INTERACTABLE_ENTITIES = false;
        public static bool DEBUG_DRAW_REGROWTH_GROWTHSTEPS = false;
        public static bool DEBUG_DRAW_REGROWTH_EVENT_PROGRESS = false;
        public static bool DEBUG_DRAW_DECAY = false;
        public static bool DEBUG_DRAW_OWNERSHIP_CURRENT_SECTOR = false;
        public static bool DEBUG_DRAW_OWNERSHIP_CURRENT_ANGLES = false;
        public static bool DEBUG_DRAW_OWNERSHIP_SECTOR_ANGLES = false;
        public static int DEBUG_DRAW_OWNERSHIP_DRAW_DISTANCE = 0x3e8;
        public static bool DEBUG_DRAW_OWNERSHIP_SECTORS_STATUS = false;
        public static bool DEBUG_DRAW_INVERSE_KINEMATICS = false;
        public static bool DEBUG_DRAW_CHARACTER_TOOLS = false;
        public static bool DEBUG_DRAW_VELOCITIES = false;
        public static bool DEBUG_DRAW_INTERPOLATED_VELOCITIES = false;
        public static bool DEBUG_DRAW_RIGID_BODY_ACTIONS = false;
        public static bool DEBUG_DRAW_TOI_OPTIMIZED_GRIDS = false;
        public static bool DEBUG_DRAW_NETWORK_SYNC = false;
        public static bool DEBUG_DRAW_HUD = false;
        public static bool DEBUG_DRAW_SERVER_WARNINGS = false;
        public static bool DEBUG_DRAW_VOXEL_CONTACT_MATERIAL = false;
        public static bool DEBUG_DRAW_TREE_COLLISION_SHAPES = false;
        public const bool DEBUG_DRAW_GRID_DEFORMATIONS = false;
        public static bool DEBUG_DRAW_ASTEROID_SEEDS = true;
        public static bool DEBUG_DRAW_ASTEROID_ORES = false;
        public static bool DEBUG_DRAW_ASTEROID_COMPOSITION = false;
        public static bool DEBUG_DRAW_ASTEROID_COMPOSITION_CONTENT = false;
        public static bool DEBUG_DRAW_ENCOUNTERS = false;
        public static bool DEBUG_DRAW_IGC = false;
        public static bool DEBUG_DRAW_VOXEL_ACCESS = false;
        public static bool DEBUG_DRAW_VOXEL_FULLCELLS = false;
        public static bool DEBUG_DRAW_VOXEL_CONTENT_MICRONODES = false;
        public static bool DEBUG_DRAW_VOXEL_CONTENT_MICRONODES_SCALED = false;
        public static bool DEBUG_DRAW_VOXEL_CONTENT_MACRONODES = false;
        public static bool DEBUG_DRAW_VOXEL_CONTENT_MACROLEAVES = false;
        public static bool DEBUG_DRAW_VOXEL_CONTENT_MACRO_SCALED = false;
        public static bool DEBUG_DRAW_VOXEL_MATERIALS_MACRONODES = false;
        public static bool DEBUG_DRAW_VOXEL_MATERIALS_MACROLEAVES = false;
        public static bool DEBUG_DRAW_JOYSTICK_CONTROL_HINTS = false;

        public static bool ENABLE_DEBUG_DRAW
        {
            get => 
                m_enableDebugDraw;
            set
            {
                m_enableDebugDraw = value;
                if (!m_enableDebugDraw)
                {
                    MyRenderProxy.DebugClearPersistentMessages();
                }
            }
        }
    }
}

