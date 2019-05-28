namespace Sandbox.Engine.Utils
{
    using System;
    using VRage.Utils;

    internal static class MyEnumsToStrings
    {
        public static string[] HudTextures;
        public static string[] Particles;
        public static string[] HudRadarTextures;
        public static string[] Decals;
        public static string[] CockpitGlassDecals;
        public static string[] SessionType;

        static MyEnumsToStrings()
        {
            string[] textArray1 = new string[13];
            textArray1[0] = "corner.png";
            textArray1[1] = "crosshair.png";
            textArray1[2] = "HudOre.png";
            textArray1[3] = "Target_enemy.png";
            textArray1[4] = "Target_friend.png";
            textArray1[5] = "Target_neutral.png";
            textArray1[6] = "Target_me.png";
            textArray1[7] = "TargetTurret.png";
            textArray1[8] = "DirectionIndicator.png";
            textArray1[9] = "gravity_point_red.png";
            textArray1[10] = "gravity_point_white.png";
            textArray1[11] = "gravity_arrow.png";
            textArray1[12] = "hit_confirmation.png";
            HudTextures = textArray1;
            string[] textArray2 = new string[0x4e];
            textArray2[0] = "Explosion.dds";
            textArray2[1] = "ExplosionSmokeDebrisLine.dds";
            textArray2[2] = "Smoke.dds";
            textArray2[3] = "Test.dds";
            textArray2[4] = "EngineThrustMiddle.dds";
            textArray2[5] = "ReflectorCone.dds";
            textArray2[6] = "ReflectorGlareAdditive.dds";
            textArray2[7] = "ReflectorGlareAlphaBlended.dds";
            textArray2[8] = "MuzzleFlashMachineGunFront.dds";
            textArray2[9] = "MuzzleFlashMachineGunSide.dds";
            textArray2[10] = "ProjectileTrailLine.dds";
            textArray2[11] = "ContainerBorder.dds";
            textArray2[12] = "Dust.dds";
            textArray2[13] = "Crosshair.dds";
            textArray2[14] = "Sun.dds";
            textArray2[15] = "LightRay.dds";
            textArray2[0x10] = "LightGlare.dds";
            textArray2[0x11] = "SolarMapOrbitLine.dds";
            textArray2[0x12] = "SolarMapSun.dds";
            textArray2[0x13] = "SolarMapAsteroidField.dds";
            textArray2[20] = "SolarMapFactionMap.dds";
            textArray2[0x15] = "SolarMapAsteroid.dds";
            textArray2[0x16] = "SolarMapZeroPlaneLine.dds";
            textArray2[0x17] = "SolarMapSmallShip.dds";
            textArray2[0x18] = "SolarMapLargeShip.dds";
            textArray2[0x19] = "SolarMapOutpost.dds";
            textArray2[0x1a] = "Grid.dds";
            textArray2[0x1b] = "ContainerBorderSelected.dds";
            textArray2[0x1c] = "FactionRussia.dds";
            textArray2[0x1d] = "FactionChina.dds";
            textArray2[30] = "FactionJapan.dds";
            textArray2[0x1f] = "FactionUnitedKorea.dds";
            textArray2[0x20] = "FactionFreeAsia.dds";
            textArray2[0x21] = "FactionSaudi.dds";
            textArray2[0x22] = "FactionEAC.dds";
            textArray2[0x23] = "FactionCSR.dds";
            textArray2[0x24] = "FactionIndia.dds";
            textArray2[0x25] = "FactionChurch.dds";
            textArray2[0x26] = "FactionOmnicorp.dds";
            textArray2[0x27] = "FactionFourthReich.dds";
            textArray2[40] = "FactionSlavers.dds";
            textArray2[0x29] = "Smoke_b.dds";
            textArray2[0x2a] = "Smoke_c.dds";
            textArray2[0x2b] = "Sparks_a.dds";
            textArray2[0x2c] = "Sparks_b.dds";
            textArray2[0x2d] = "particle_stone.dds";
            textArray2[0x2e] = "Stardust.dds";
            textArray2[0x2f] = "particle_trash_a.dds";
            textArray2[0x30] = "particle_trash_b.dds";
            textArray2[0x31] = "particle_glare.dds";
            textArray2[50] = "smoke_field.dds";
            textArray2[0x33] = "Explosion_pieces.dds";
            textArray2[0x34] = "particle_laser.dds";
            textArray2[0x35] = "particle_nuclear.dds";
            textArray2[0x36] = "Explosion_line.dds";
            textArray2[0x37] = "particle_flash_a.dds";
            textArray2[0x38] = "particle_flash_b.dds";
            textArray2[0x39] = "particle_flash_c.dds";
            textArray2[0x3a] = "snap_point.dds";
            textArray2[0x3b] = "SolarMapNavigationMark.dds";
            textArray2[60] = "Impostor_StaticAsteroid20m_A.dds";
            textArray2[0x3d] = "Impostor_StaticAsteroid20m_C.dds";
            textArray2[0x3e] = "Impostor_StaticAsteroid50m_D.dds";
            textArray2[0x3f] = "Impostor_StaticAsteroid50m_E.dds";
            textArray2[0x40] = "GPS.dds";
            textArray2[0x41] = "GPSBack.dds";
            textArray2[0x42] = "ShotgunParticle.dds";
            textArray2[0x43] = "ObjectiveDummyFace.dds";
            textArray2[0x44] = "ObjectiveDummyLine.dds";
            textArray2[0x45] = "SunDisk.dds";
            textArray2[70] = "scanner_01.dds";
            textArray2[0x47] = "Smoke_square.dds";
            textArray2[0x48] = "Smoke_lit.dds";
            textArray2[0x49] = "SolarMapSideMission.dds";
            textArray2[0x4a] = "SolarMapStoryMission.dds";
            textArray2[0x4b] = "SolarMapTemplateMission.dds";
            textArray2[0x4c] = "SolarMapPlayer.dds";
            textArray2[0x4d] = "ReflectorConeCharacter.dds";
            Particles = textArray2;
            string[] textArray3 = new string[0x1a];
            textArray3[0] = "Arrow.png";
            textArray3[1] = "ImportantObject.tga";
            textArray3[2] = "LargeShip.tga";
            textArray3[3] = "Line.tga";
            textArray3[4] = "RadarBackground.tga";
            textArray3[5] = "RadarPlane.tga";
            textArray3[6] = "SectorBorder.tga";
            textArray3[7] = "SmallShip.tga";
            textArray3[8] = "Sphere.png";
            textArray3[9] = "SphereGrid.tga";
            textArray3[10] = "Sun.tga";
            textArray3[11] = "OreDeposit_Treasure.png";
            textArray3[12] = "OreDeposit_Helium.png";
            textArray3[13] = "OreDeposit_Ice.png";
            textArray3[14] = "OreDeposit_Iron.png";
            textArray3[15] = "OreDeposit_Lava.png";
            textArray3[0x10] = "OreDeposit_Gold.png";
            textArray3[0x11] = "OreDeposit_Platinum.png";
            textArray3[0x12] = "OreDeposit_Silver.png";
            textArray3[0x13] = "OreDeposit_Silicon.png";
            textArray3[20] = "OreDeposit_Organic.png";
            textArray3[0x15] = "OreDeposit_Nickel.png";
            textArray3[0x16] = "OreDeposit_Magnesium.png";
            textArray3[0x17] = "OreDeposit_Uranite.png";
            textArray3[0x18] = "OreDeposit_Cobalt.png";
            textArray3[0x19] = "OreDeposit_Snow.png";
            HudRadarTextures = textArray3;
            Decals = new string[] { "ExplosionSmut", "BulletHoleOnMetal", "BulletHoleOnRock" };
            CockpitGlassDecals = new string[] { "DirtOnGlass", "BulletHoleOnGlass", "BulletHoleSmallOnGlass" };
            string[] textArray6 = new string[11];
            textArray6[0] = "NEW_STORY";
            textArray6[1] = "LOAD_CHECKPOINT";
            textArray6[2] = "JOIN_FRIEND_STORY";
            textArray6[3] = "MMO";
            textArray6[4] = "SANDBOX_OWN";
            textArray6[5] = "SANDBOX_FRIENDS";
            textArray6[6] = "JOIN_SANDBOX_FRIEND";
            textArray6[7] = "EDITOR_SANDBOX";
            textArray6[8] = "EDITOR_STORY";
            textArray6[9] = "EDITOR_MMO";
            textArray6[10] = "SANDBOX_RANDOM";
            SessionType = textArray6;
        }

        private static void Validate<T>(Type type, T list) where T: IList<string>
        {
            Array values = Enum.GetValues(type);
            Type underlyingType = Enum.GetUnderlyingType(type);
            if (underlyingType == typeof(byte))
            {
                foreach (byte num in values)
                {
                    MyDebug.AssertRelease(list[num] != null);
                }
            }
            else if (underlyingType == typeof(short))
            {
                foreach (short num2 in values)
                {
                    MyDebug.AssertRelease(list[num2] != null);
                }
            }
            else if (underlyingType == typeof(ushort))
            {
                foreach (ushort num3 in values)
                {
                    MyDebug.AssertRelease(list[num3] != null);
                }
            }
            else if (!(underlyingType == typeof(int)))
            {
                throw new InvalidBranchException();
            }
            else
            {
                foreach (int num4 in values)
                {
                    MyDebug.AssertRelease(list[num4] != null);
                }
            }
        }
    }
}

