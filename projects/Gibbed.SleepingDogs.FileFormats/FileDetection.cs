/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Gibbed.SleepingDogs.FileFormats
{
    public class FileDetection
    {
        private struct MagicInfo
        {
            public uint Magic;
            public string Directory;
            public string Extension;

            public MagicInfo(uint magic, string directory, string extension)
            {
                this.Magic = magic;
                this.Directory = directory;
                this.Extension = extension;
            }
        }

        private static readonly ReadOnlyCollection<MagicInfo> _Extensions =
            new ReadOnlyCollection<MagicInfo>(new[]
            {
                new MagicInfo(0x0C46AEEF, "shaders", "template"), // ShaderTemplateInventory
                new MagicInfo(0x1418DD74, "inventories", "RigInventory"),
                new MagicInfo(0x2C40FA26, "inventories", "UniqueUIDTableResourceInventory"),
                new MagicInfo(0x43425844, "shaders", "bin"),
                new MagicInfo(0x442A39D9, "inventories", "UIScreenInventory"),
                new MagicInfo(0x5E73CDD7, "textures", "texturepack.bin"),
                new MagicInfo(0x5B9BF81E, "inventories", "PropertySetInventory"),
                new MagicInfo(0x7A971479, "inventories", "BufferInventory"),
                new MagicInfo(0x8ACF9964, "inventories", "AnimationInventory"),
                new MagicInfo(0x90CE6B7A, "inventories", "UILocalizationChunkInventory"),
                new MagicInfo(0x985BE50C, "shaders", "template.bin"), // ShaderBinaryInventory
                new MagicInfo(0xAEDF1081, "inventories", "ImposterGroupInventory"),
                new MagicInfo(0xAF015A94, "inventories", "StateBlockInventory"),
                new MagicInfo(0x580501C9, "inventories", "CameraResourceInventory"),
                new MagicInfo(0xBD226A08, "inventories", "CollisionMeshBundleInventory"),
                new MagicInfo(0xCDBFA090, "inventories", "TextureInventory"),
                new MagicInfo(0xD05B6976, "inventories", "ParticleEmitterSettingsInventory"),
                new MagicInfo(0xE2C5C78C, "inventories", "BSPDebugDataInventory"),
                new MagicInfo(0xE5150CC0, "inventories", "DynamicCoverDataInventory"),
                new MagicInfo(0xE7F23AEE, "inventories", "SceneLayerInventory"),
                new MagicInfo(0xF5F8516F, "inventories", "MaterialInventory"),
            });

        public static KeyValuePair<string, string> Detect(byte[] guess, int read)
        {
            if (read == 0)
            {
                return new KeyValuePair<string, string>("null", "null");
            }

            if (read >= 4)
            {
                var magic = BitConverter.ToUInt32(guess, 0);
                var match = _Extensions.FirstOrDefault(e => e.Magic == magic);
                if (match.Directory != null && match.Extension != null)
                {
                    return new KeyValuePair<string, string>(match.Directory, match.Extension);
                }
            }

            if (read >= 8 &&
                guess[4] == 0xCB &&
                guess[5] == 0xC0 &&
                guess[6] == 0xDE)
            {
                return new KeyValuePair<string, string>("scripts", "skoo-bin");
            }

            return new KeyValuePair<string, string>("unknown", "unknown");
        }
    }
}
