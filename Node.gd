extends Node


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	var x = load("res://addons/WAT/core/assertions/Asserts.cs").new()
	print(x.get("assertions"))
	print(x.get("IS_WAT_SUITE"))
	print(x.get_words())
