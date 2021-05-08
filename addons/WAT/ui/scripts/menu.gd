extends Control
tool

onready var QuickRunAll: Button = $QuickRunAll

# Loads scaled assets like icons and fonts
func _setup_editor_assets(assets_registry):
	QuickRunAll.icon = assets_registry.load_asset(QuickRunAll.icon)
