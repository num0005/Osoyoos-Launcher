using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ToolkitLauncher.ToolkitInterface;

namespace ToolkitLauncher.Utility
{
    internal class ReachToolPatcher
    {
        // Disables "color->red>=0" type assertion failures during lightmapping
        // applyPatch being true means we edit the original exe, it being false means we revert the changes
        public static void PatchLightmapColorAssert(bool applyPatch, string toolPath, string toolFastPath)
        {
            byte[] newBytes = [0x90, 0x90, 0x90, 0x90, 0x90];

            // Reach tool.exe/tool_fast.exe 18/07/2023
            // For each Tool type, holds the original unmodified region hash, the patched region hash, and a list of patch location addresses and the original bytes needed to reverse the patch
            // The precalculated hashes here are obtained directly from ComputeRegionHash(), don't try to update these manually in the future
            var patchMap = new Dictionary<ToolType, (string original, string patched, (long offset, byte[] RevertBytes)[] locations)>
            {
                {
                    ToolType.Tool,
                    (
                        "95CF79AC2308FE54D8003316E71223C3B789228E2EAB453B2C09CE03813B8247", // original
                        "04C44C75D6456E396200B0F048299DF478509ABB6089047D5783523D06B344E4", // patched
                        new (long, byte[])[]
                        {
                            ( 0x17095F, new byte[] { 0xE8, 0x5C, 0x87, 0x68, 0x00 }),
                            ( 0x170975, new byte[] { 0xE8, 0x46, 0x87, 0x68, 0x00 }),
                            ( 0x170990, new byte[] { 0xE8, 0x2B, 0x87, 0x68, 0x00 }),
                            ( 0x171588, new byte[] { 0xE8, 0x33, 0x7B, 0x68, 0x00 }),
                            ( 0x17159E, new byte[] { 0xE8, 0x1D, 0x7B, 0x68, 0x00 }),
                            ( 0x1715B9, new byte[] { 0xE8, 0x02, 0x7B, 0x68, 0x00 })
                        }
                    )
                },
                {
                    ToolType.ToolFast,
                    (
                        "C59F3287BF1ED5C7FFF7306FFD583F98CBF4F3A85D12E13313F565A24566DD47", // original
                        "FE232C0F1EA8CF9AB5380F3DCBF20781D5E68C41D22AA10A21B043EDAE18551A", // patched
                        new (long, byte[])[]
                        {
                            ( 0xF2A02, new byte[] { 0xE8, 0x4D, 0x54, 0x29, 0x00 }),
                            ( 0xF2A18, new byte[] { 0xE8, 0x37, 0x54, 0x29, 0x00 }),
                            ( 0xF2A33, new byte[] { 0xE8, 0x1C, 0x54, 0x29, 0x00 }),
                            ( 0xF2A88, new byte[] { 0xE8, 0xC7, 0x53, 0x29, 0x00 }),
                            ( 0xF2A9E, new byte[] { 0xE8, 0xB1, 0x53, 0x29, 0x00 }),
                            ( 0xF2AB9, new byte[] { 0xE8, 0x96, 0x53, 0x29, 0x00 })
                        }
                    )
                }
            };

            var toolPaths = new[]
            {
                (Type: ToolType.Tool, Path: toolPath),
                (Type: ToolType.ToolFast, Path: toolFastPath)
            };

            // Loop through both exe types
            foreach (var (toolType, exePath) in toolPaths)
            {
                if (!File.Exists(exePath))
                {
                    continue;
                }

                // Verify hash before patching
                var patchData = patchMap[toolType];
                string fileHash = ComputeRegionHash(exePath, patchData.locations.Select(x => x.offset), 1024);

                // Exit early if patch shouldn't be applied
                if (!ShouldPatch(fileHash, patchData.original, patchData.patched, Path.GetFileName(exePath), applyPatch))
                {
                    return;
                }

                // Perform patching/reverting
                try
                {
                    ToolPatcher(exePath, patchData.locations.Select(loc => (loc.offset, applyPatch ? newBytes : loc.RevertBytes)));
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Failed to patch {exePath}: {ex.Message}", "Patch Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            static void ToolPatcher(string exePath, IEnumerable<(long offset, byte[] bytes)> patches)
            {
                using var fs = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                foreach (var (offset, bytes) in patches)
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            static string ComputeRegionHash(string exePath, IEnumerable<long> offsets, int regionSize)
            {
                using var sha256 = SHA256.Create();
                using var fs = new FileStream(exePath, FileMode.Open, FileAccess.Read);
                using var ms = new MemoryStream();

                // Gather all region bytes together
                foreach (var offset in offsets)
                {
                    long regionStart = Math.Max(0, offset - regionSize / 2); // Center region on the offset
                    byte[] buffer = new byte[regionSize];

                    fs.Seek(regionStart, SeekOrigin.Begin);
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);

                    ms.Write(buffer, 0, bytesRead);
                }

                // Hash the joined regions
                byte[] hash = sha256.ComputeHash(ms.ToArray());
                return BitConverter.ToString(hash).Replace("-", "");
            }

            static bool ShouldPatch(string fileHash, string originalHash, string patchedHash, string exeName, bool applyPatch)
            {
                if (applyPatch)
                {
                    if (fileHash.Equals(originalHash, StringComparison.OrdinalIgnoreCase))
                    {
                        return true; // Safe to patch original file
                    }

                    if (fileHash.Equals(patchedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        return false; // Already patched, skip
                    }
                }
                else
                {
                    if (fileHash.Equals(patchedHash, StringComparison.OrdinalIgnoreCase))
                    {
                        return true; // Safe to revert
                    }

                    if (fileHash.Equals(originalHash, StringComparison.OrdinalIgnoreCase))
                    {
                        return false; // Already unpatched
                    }
                }

                // Unknown file changes
                Trace.WriteLine($"Region hash mismatch for {exeName} -- aborting patch");
                MessageBox.Show($"Region hash mismatch for {exeName} -- aborting \"color assert\" patch.\nUnknown modification detected in the 1KB surrounding patch region(s)\nLightmapping will continue but you may experience a crash.",
                "Patcher Error", MessageBoxButton.OK, MessageBoxImage.Error
                );
                return false;
            }
        }
    }
}
