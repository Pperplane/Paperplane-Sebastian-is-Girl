using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewModdingAPI;

namespace SebastianToSabrina
{
    [HarmonyPatch(typeof(IslandSouth), nameof(IslandSouth.SetupIslandSchedules))]
    public class IslandSouthPatch
    {
        
        [HarmonyPrefix]
        public static bool SetupIslandSchedules_Prefix()
        {
            try
            {
                Game1.netWorldState.Value.IslandVisitors.Clear();

                if (Utility.isFestivalDay() || Utility.IsPassiveFestivalDay() || 
                    !(Game1.getLocationFromName("IslandSouth") is IslandSouth island) || 
                    !island.resortRestored.Value || island.IsRainingHere() || !island.resortOpenToday.Value)
                {
                    return false; 
                }

                Random seeded_random = Utility.CreateRandom((double)Game1.uniqueIDForThisGame * 1.21, (double)Game1.stats.DaysPlayed * 2.5);
                List<NPC> valid_visitors = new List<NPC>();

                // REFLECTION: Set up access to private 'CanVisitIslandToday' method
                var canVisitMethod = AccessTools.Method(typeof(IslandSouth), "CanVisitIslandToday", new Type[] { typeof(NPC) });

                Utility.ForEachVillager(delegate (NPC npc)
                {
                    // Invoke the private method safely
                    if ((bool)canVisitMethod.Invoke(null, new object[] { npc }))
                    {
                        valid_visitors.Add(npc);
                    }
                    return true;
                });

                List<NPC> visitors = new List<NPC>();

                if (seeded_random.NextDouble() < 0.4)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        NPC visitor = seeded_random.ChooseFrom(valid_visitors);
                        if (visitor != null && visitor.Age != 2)
                        {
                            valid_visitors.Remove(visitor);
                            visitors.Add(visitor);
                            visitor.scheduleDelaySeconds = Math.Min((float)i * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
                        }
                    }
                }
                else
                {
                    List<string>[] potentialGroups = new List<string>[12]
                    {
                        new List<string> { "Sebastian", "Sam", "Abigail" },
                        new List<string> { "Jodi", "Kent", "Vincent", "Sam" },
                        new List<string> { "Jodi", "Vincent", "Sam" },
                        new List<string> { "Pierre", "Caroline", "Abigail" },
                        new List<string> { "Robin", "Demetrius", "Maru", "Sebastian" },
                        new List<string> { "Lewis", "Marnie" },
                        new List<string> { "Marnie", "Shane", "Jas" },
                        new List<string> { "Penny", "Jas", "Vincent" },
                        new List<string> { "Pam", "Penny" },
                        new List<string> { "Caroline", "Marnie", "Robin", "Jodi" },
                        // MODIFIED: Sebastian added to the girls' resort group list
                        new List<string> { "Haley", "Penny", "Leah", "Emily", "Maru", "Abigail", "Sebastian" },
                        new List<string> { "Alex", "Sam", "Elliott", "Shane", "Harvey" }
                    };

                    List<string> group = potentialGroups[seeded_random.Next(potentialGroups.Length)];
                    bool failed = false;

                    foreach (string s in group)
                    {
                        if (!valid_visitors.Contains(Game1.getCharacterFromName(s)))
                        {
                            failed = true;
                            break;
                        }
                    }

                    if (!failed)
                    {
                        int i2 = 0;
                        foreach (string item in group)
                        {
                            NPC visitor2 = Game1.getCharacterFromName(item);
                            valid_visitors.Remove(visitor2);
                            visitors.Add(visitor2);
                            visitor2.scheduleDelaySeconds = Math.Min((float)i2 * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
                            i2++;
                        }
                    }

                    for (int i3 = 0; i3 < 5 - visitors.Count; i3++)
                    {
                        NPC visitor3 = seeded_random.ChooseFrom(valid_visitors);
                        if (visitor3 != null && visitor3.Age != 2)
                        {
                            valid_visitors.Remove(visitor3);
                            visitors.Add(visitor3);
                            visitor3.scheduleDelaySeconds = Math.Min((float)i3 * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
                        }
                    }
                }

                List<IslandSouth.IslandActivityAssigments> activities = new List<IslandSouth.IslandActivityAssigments>();
                Dictionary<Character, string> last_activity_assignments = new Dictionary<Character, string>();
                activities.Add(new IslandSouth.IslandActivityAssigments(1200, visitors, seeded_random, last_activity_assignments));
                activities.Add(new IslandSouth.IslandActivityAssigments(1400, visitors, seeded_random, last_activity_assignments));
                activities.Add(new IslandSouth.IslandActivityAssigments(1600, visitors, seeded_random, last_activity_assignments));

                // //TODO: Remove this on deployment
                // ModEntry.ModMonitor.Log($"Visitors:", LogLevel.Debug);
                // foreach (var visitorItem in visitors)
                // {
                //     ModEntry.ModMonitor.Log($"- {visitorItem.Name}", LogLevel.Debug);
                // }
                // REFLECTION: Set up access to private 'HasIslandAttire' and 'GetDressingRoomPoint' methods
                var hasAttireMethod = AccessTools.Method(typeof(IslandSouth), "HasIslandAttire", new Type[] { typeof(NPC) });
                var getDressingRoomMethod = AccessTools.Method(typeof(IslandSouth), "GetDressingRoomPoint", new Type[] { typeof(NPC) });

                foreach (NPC visitor4 in visitors)
                {
                    StringBuilder schedule = new StringBuilder("");
                    bool should_dress = (bool)hasAttireMethod.Invoke(null, new object[] { visitor4 });
                    if (visitor4.Name == "Sebastian")
                    {
                        should_dress = true; // Force her to change clothes/swimsuit
                    }
                    bool had_first_activity = false;

                    if (should_dress)
                    {
                        Point dressing_room;
                        if (visitor4.Name == "Sebastian")
                        {
                            dressing_room = new Point(22, 19); // Female dressing room
                            ModEntry.ModMonitor.Log("[Sebastian To Sabrina] Forcing female dressing room point (22, 19)", StardewModdingAPI.LogLevel.Debug);
                        }
                        else
                        {
                            dressing_room = (Point)getDressingRoomMethod.Invoke(null, new object[] { visitor4 });
                        }

                        schedule.Append("/a1150 IslandSouth " + dressing_room.X + " " + dressing_room.Y + " change_beach");
                        had_first_activity = true;
                    }

                    foreach (IslandSouth.IslandActivityAssigments item2 in activities)
                    {
                        string current_string = item2.GetScheduleStringForCharacter(visitor4);
                        if (current_string != "")
                        {
                            if (!had_first_activity)
                            {
                                current_string = "/a" + current_string.Substring(1);
                                had_first_activity = true;
                                }
                            schedule.Append(current_string);
                        }
                    }

                    if (should_dress)
                    {
                        // Do the same check for when they leave the beach
                        Point dressing_room2;
                        if (visitor4.Name == "Sebastian")
                        {
                            dressing_room2 = new Point(22, 19);
                        }
                        else
                        {
                            dressing_room2 = (Point)getDressingRoomMethod.Invoke(null, new object[] { visitor4 });
                        }

                        schedule.Append("/a1730 IslandSouth " + dressing_room2.X + " " + dressing_room2.Y + " change_normal");
                    }

                    if (visitor4.Name == "Gus")
                    {
                        schedule.Append("/1800 Saloon 10 18 2/2430 bed");
                    }
                    else
                    {
                        schedule.Append("/1800 bed");
                    }

                    schedule.Remove(0, 1);

                    if (visitor4.TryLoadSchedule("island", schedule.ToString()))
                    {
                        visitor4.islandScheduleName.Value = "island";
                        Game1.netWorldState.Value.IslandVisitors.Add(visitor4.Name);
                    }
                    visitor4.performSpecialScheduleChanges();
                }
            }
            catch (Exception ex)
            {
                // Better logging for SMAPI terminal
                Console.WriteLine($"[Sebastian To Sabrina] Error overriding SetupIslandSchedules: {ex}");
            }

            return false; 
        }
    }
}