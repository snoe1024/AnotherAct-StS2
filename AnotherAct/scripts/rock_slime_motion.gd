extends Node2D

const ASSET = "res://AnotherAct/animations/monsters/rock_slime/motions_trial_06/"
const NAMES = ["idle_loop", "attack", "cast", "hurt", "die"]
var character: Dictionary
var mesh_asset: Dictionary
var motions: Dictionary = {}
var textures: Dictionary = {}
var body: Polygon2D
var lip: Polygon2D
var rocks: Array[Sprite2D] = []
var action := "idle_loop"
var elapsed := 0.0
var dead := false
var vertices := PackedVector2Array()
var speed_provider := Callable()
var playback_speed := 1.0

func read_json(path: String) -> Dictionary:
	assert(FileAccess.file_exists(path), "Missing rock slime data: " + path)
	var value = JSON.parse_string(FileAccess.get_file_as_string(path))
	assert(value is Dictionary, "Invalid rock slime JSON: " + path)
	return value

func texture(path: String) -> Texture2D:
	if not textures.has(path):
		textures[path] = load(ASSET + path)
	assert(textures[path] is Texture2D, "Missing rock slime texture: " + path)
	return textures[path]

func vec(value: Array) -> Vector2:
	return Vector2(value[0], value[1])

func make_mesh() -> Polygon2D:
	var node := Polygon2D.new()
	var uv := PackedVector2Array()
	var polygons: Array[PackedInt32Array] = []
	for point in mesh_asset.topology.vertices:
		uv.append(vec(point))
	for triangle in mesh_asset.topology.triangles:
		polygons.append(PackedInt32Array(triangle))
	node.polygon = uv
	node.uv = uv
	node.polygons = polygons
	add_child(node)
	return node

func _ready() -> void:
	character = read_json(ASSET + "character.json")
	mesh_asset = read_json(ASSET + "mesh_asset.json")
	for name in NAMES:
		motions[name] = read_json(ASSET + "baked/" + name + ".json")
	body = make_mesh()
	var shader := Shader.new()
	shader.code = "shader_type canvas_item; uniform sampler2D revealed : source_color; uniform float mixture; varying vec4 tint; void vertex(){ tint = COLOR; } void fragment(){ COLOR = mix(texture(TEXTURE, UV), texture(revealed, UV), mixture) * tint; }"
	var material := ShaderMaterial.new()
	material.shader = shader
	body.material = material
	for i in range(8):
		var rock := Sprite2D.new()
		rock.centered = false
		add_child(rock)
		rocks.append(rock)
	lip = make_mesh()
	lip.texture = texture(character.front_lip)
	render_at("idle_loop", 0.0)

func play_motion(name: String) -> void:
	# Death is terminal, including late Idle/Attack triggers and repeated Dead calls.
	if dead:
		return
	assert(motions.has(name))
	action = name
	elapsed = 0.0
	dead = name == "die"
	# Capture the mode at the trigger, just as the game's attack wait does.
	playback_speed = float(speed_provider.call(name)) if speed_provider.is_valid() else 1.0
	if playback_speed <= 0.0:
		elapsed = motions[name].frames[-1].time
		render_at(name, elapsed)
		if not dead:
			action = "idle_loop"
			elapsed = 0.0
			playback_speed = 1.0
			render_at(action, elapsed)
		return
	render_at(action, elapsed)

func _process(delta: float) -> void:
	elapsed += delta * playback_speed
	var frames: Array = motions[action].frames
	var duration: float = frames[-1].time
	if action == "idle_loop":
		elapsed = fmod(elapsed, duration)
	elif elapsed >= duration and not dead:
		action = "idle_loop"
		elapsed = 0.0
		playback_speed = 1.0
	render_at(action, minf(elapsed, motions[action].frames[-1].time))

func render_at(name: String, time: float) -> void:
	var motion: Dictionary = motions[name]
	var frames: Array = motion.frames
	var index := 0
	while index + 1 < frames.size() and frames[index + 1].time <= time:
		index += 1
	var a: Dictionary = frames[index]
	var b: Dictionary = frames[mini(index + 1, frames.size() - 1)]
	var weight := 0.0
	if b.time > a.time:
		weight = clampf((time - a.time) / (b.time - a.time), 0.0, 1.0)
	var root := vec(a.root_translation).lerp(vec(b.root_translation), weight)
	vertices.clear()
	for i in range(a.body_vertices.size()):
		vertices.append(vec(a.body_vertices[i]).lerp(vec(b.body_vertices[i]), weight) + root)
	# Body vertices are local; rigid positions in the bake already include root.
	body.polygon = vertices
	lip.polygon = vertices
	var profile: Dictionary = character.material_profiles.get(motion.material_profile, {})
	body.texture = texture(profile.get("body", character.parts[0].image))
	var material := body.material as ShaderMaterial
	material.set_shader_parameter("revealed", texture(profile.get("revealed_body", character.revealed_body)))
	material.set_shader_parameter("mixture", lerpf(a.body_texture_mix, b.body_texture_mix, weight))
	lip.visible = a.foreground_occlusion
	assert(a.rigid_instances.size() <= rocks.size())
	for i in range(rocks.size()):
		var rock := rocks[i]
		rock.visible = i < a.rigid_instances.size()
		if not rock.visible:
			continue
		var inst: Dictionary = a.rigid_instances[i]
		var next: Dictionary = inst
		if i < b.rigid_instances.size() and b.rigid_instances[i].part == inst.part and b.rigid_instances[i].state == inst.state:
			next = b.rigid_instances[i]
		var part: Dictionary = {}
		for candidate in character.parts:
			if candidate.id == inst.part:
				part = candidate
		assert(not part.is_empty())
		rock.texture = texture(profile.get("rigid_images", {}).get(part.id, part.image))
		rock.offset = vec(part.position) - vec(part.pivot)
		rock.position = vec(inst.position).lerp(vec(next.position), weight)
		rock.rotation = deg_to_rad(lerpf(inst.rotation_deg, next.rotation_deg, weight))
		rock.scale = vec(inst.scale).lerp(vec(next.scale), weight)
		rock.modulate.a = lerpf(inst.opacity, next.opacity, weight)
