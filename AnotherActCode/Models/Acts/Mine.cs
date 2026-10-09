using AnotherAct.AnotherActCode.Extensions;
using AnotherAct.AnotherActCode.Models.Encounters;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;

namespace AnotherAct.AnotherActCode.Models.Acts;

public sealed class Mine : CustomActModel
{
	public Mine() : base(1) { }
	
	public override IEnumerable<EncounterModel> GenerateAllEncounters()
	{
		// NOTE:
		// CustomEncounterModelを使用している場合はBaseLibが勝手に追加する
		// バニラのエンカウントは敵のバリエーションを意識するために残しておく
		// 追加するたびに一つづつ置き換えてここを空に出来たらうれしいね
		return [
			// ModelDb.Encounter<DrillMachineWeak>(),
			// ModelDb.Encounter<MineSlimesWeak>(),
			// ModelDb.Encounter<BoulderMiteWeak>(),
			ModelDb.Encounter<ShrinkerBeetleWeak>(),
			
			// ModelDb.Encounter<CubexConstructNormal>(),
			// ModelDb.Encounter<FlyconidNormal>(),
			// ModelDb.Encounter<FogmogNormal>(),
			ModelDb.Encounter<InkletsNormal>(),
			ModelDb.Encounter<MawlerNormal>(),
			ModelDb.Encounter<NibbitsNormal>(),
			ModelDb.Encounter<OvergrowthCrawlers>(),
			ModelDb.Encounter<RubyRaidersNormal>(),
			// ModelDb.Encounter<SlimesNormal>(),
			ModelDb.Encounter<SlitheringStranglerNormal>(),
			ModelDb.Encounter<SnappingJaxfruitNormal>(),
			ModelDb.Encounter<VineShamblerNormal>(),
			
			// ModelDb.Encounter<BygoneEffigyElite>(),
			ModelDb.Encounter<ByrdonisElite>(),
			ModelDb.Encounter<PhrogParasiteElite>(),
			
			ModelDb.Encounter<CeremonialBeastBoss>(),
			ModelDb.Encounter<TheKinBoss>(),
			ModelDb.Encounter<VantomBoss>(),
		];
	}
	
	public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act1";

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => 
	[
		ModelDb.Encounter<VantomBoss>(),
		ModelDb.Encounter<CeremonialBeastBoss>(),
		ModelDb.Encounter<TheKinBoss>()
	];

	public override IEnumerable<EventModel> AllEvents => 
	[
		ModelDb.Event<AromaOfChaos>(),
		ModelDb.Event<ByrdonisNest>(),
		ModelDb.Event<DenseVegetation>(),
		ModelDb.Event<JungleMazeAdventure>(),
		ModelDb.Event<LuminousChoir>(),
		ModelDb.Event<MorphicGrove>(),
		ModelDb.Event<SapphireSeed>(),
		ModelDb.Event<SunkenStatue>(),
		ModelDb.Event<TabletOfTruth>(),
		ModelDb.Event<UnrestSite>(),
		ModelDb.Event<Wellspring>(),
		ModelDb.Event<WhisperingHollow>(),
		ModelDb.Event<WoodCarvings>()
	];

	protected override int NumberOfWeakEncounters => 3;
	
	protected override string CustomMapTopBgPath => $"packed/map/map_bgs/{FilePathIdentifier}/map_top_{FilePathIdentifier}.png".ImagePath();
	
	protected override string CustomMapMidBgPath => $"packed/map/map_bgs/{FilePathIdentifier}/map_middle_{FilePathIdentifier}.png".ImagePath();
	
	protected override string CustomMapBotBgPath => $"packed/map/map_bgs/{FilePathIdentifier}/map_bottom_{FilePathIdentifier}.png".ImagePath();
	
	protected override string CustomRestSiteBackgroundPath => $"rest_site/{FilePathIdentifier}_rest_site".ScenePath();

	// protected override string CustomBackgroundScenePath => "res://your_scene.tscn";
	
	// public override string? CustomChestScene => "res://your_chest_scene.tscn";

	public override string[] BgMusicOptions => ["event:/music/act1_a1_v1", "event:/music/act1_a2_v2"];
	
	public override string[] MusicBankPaths => ["res://banks/desktop/act1_a1.bank", "res://banks/desktop/act1_a2.bank"];
	
	public override string AmbientSfx => "event:/sfx/ambience/act1_ambience";

	public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_1_skel_data.tres";

	public override string ChestSpineSkinNameNormal => "act1";

	public override string ChestSpineSkinNameStroke => "act1_stroke";
	
	public override Color MapTraveledColor => new("28231D");

	public override Color MapUntraveledColor => new("877256");

	public override Color MapBgColor => new("A78A67");

	public override bool IsUnlocked(UnlockState unlockState) => true;

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
	{
		int restCount = mapRng.NextGaussianInt(7, 1, 6, 7);
		int unknownCount = MapPointTypeCounts.StandardRandomUnknownCount(mapRng);
		return new MapPointTypeCounts(unknownCount, restCount);
	}
}