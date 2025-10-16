using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

using PostProcessLayer = UnityEngine.Rendering.PostProcessing.PostProcessLayer;

namespace TexturePainter.Editor
{
	public class TexturePainterWindow : EditorWindow
	{
		[SerializeField] VisualTreeAsset layout;

		[MenuItem("Tools/Texture Painter")]
		public static void ShowWindow()
		{
			var window = GetWindow<TexturePainterWindow>();
			window.titleContent = new GUIContent("Texture Painter");
		}

		VisualElement root;

		TexturePainterToolbar toolbar => root.Q<TexturePainterToolbar>();
		Mode mode
		{
			get => toolbar.mode;
			set => toolbar.mode = value;
		}

		TexturePainterPaintablePanel paintablePanel => root.Q<TexturePainterPaintablePanel>();

		TexturePainterConfigPanel configPanel => root.Q<TexturePainterConfigPanel>();
		TexturePainterConfig config => configPanel.config;

		TexturePainterComponent component => Utils.GetTexturePainterComponent();

		//bool leftMouseDown;
		bool leftMouseHoldingDown;
		bool holdingDown_F;
		//bool leftMouseUp;
		//bool altClick;

		void CreateGUI()
		{
			root = layout.Instantiate();
			rootVisualElement.Add(root);

			root.StretchToParentSize();

			root.LoadNestedElements(layout);

			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;

			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;

			configPanel.onPropertyChanged += x => paintablePanel.Select(x);

			OnSelectionChanged();
		}

		void OnDestroy()
		{
			Selection.selectionChanged -= OnSelectionChanged;

			SceneView.duringSceneGui -= OnSceneGUI;
		}

		void OnSelectionChanged()
		{
			if (component is null)
				return;

			var gameObject = Selection.activeGameObject;
			if (gameObject)
				gameObject = gameObject.GetGameObjectWithMaterial();

			if (!gameObject)
			{
				paintablePanel.Unselect();
				configPanel.Select(null);
				return;
			}

			paintablePanel.Select(gameObject, component.GetPaintable(gameObject));
			paintablePanel.Select(config.brush.property);
			configPanel.Select(gameObject);
		}

		void Update()
		{
			if (!leftMouseHoldingDown)
				return;

			Repaint();
		}

		void ForceUse(Event e)
		{
			GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
			e.Use();
		}

		void OnSceneGUI(SceneView sceneView)
		{
			if (Application.isPlaying || mode == Mode.None || Tools.current != Tool.None)
			{
				mode = Mode.None;
				//leftMouseDown = false;
				leftMouseHoldingDown = false;
				holdingDown_F = false;
				//leftMouseUp = false;
				//altClick = false;
				return;
			}

			var e = Event.current;

			if (e.keyCode == KeyCode.F)
			{
				if (e.type == EventType.KeyDown)
				{
					holdingDown_F = true;
					ForceUse(e);
				}
				else if (e.type == EventType.KeyUp)
				{
					holdingDown_F = false;
				}
			}
			else if (e.button == 0)
			{
				switch (e.type)
				{
					case EventType.MouseDown:

						if (e.alt)
						{
							//altClick = true;
							return;
						}

						if (e.control)
							return;

						//leftMouseDown = true;
						leftMouseHoldingDown = true;
						//leftMouseUp = false;
						ForceUse(e);
						break;

					case EventType.MouseUp:
						//leftMouseDown = false;
						leftMouseHoldingDown = false;
						//leftMouseUp = true;
						break;
				}
			}

			//if (e.type != EventType.Repaint)
			//    return;

			//var camera = sceneView.camera;

			//var position = e.mousePosition;
			//position.y = camera.pixelHeight - position.y;
			//position = camera.ScreenToWorldPoint(position);
			//position.y = -position.y;

			switch (mode)
			{
				case Mode.Paint:
					OnSceneGUI_PaintMode(e);
					break;
			}

			//leftMouseDown = false;
			//leftMouseUp = false;
			//altClick = false;
		}

		void OnSceneGUI_PaintMode(Event e)
		{
			if (component.dataObject is null)
				return;

			if (e.type == EventType.ScrollWheel && e.delta.y != 0 && (e.control || e.shift))
			{
				float f = e.delta.y < 0 ? 1 : -1;
				if (e.control && e.shift)
					config.brush.IncreaseHardness(.05f * f);
				else if (e.control)
					config.brush.IncreaseAlpha(f);
				else if (e.shift)
					config.brush.IncreaseSize(f);
				e.Use();
				return;
			}

			var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
			if (!Physics.Raycast(ray, out var hit))
				return;

			// MyTODO: TryPickColor
			//if (altClick && TryPickColor(hit, out var color))
			//{
			//	var c = config.brush.color;
			//	c.r = color.r;
			//	c.g = color.g;
			//	c.b = color.b;
			//	config.brush.color = c;
			//}

			var gameObject = hit.transform.gameObject.GetGameObjectWithMaterial();
			if (!gameObject)
				return;

			var paintable = component.GetPaintable(gameObject);
			if (paintable is null)
				return;

			if (!leftMouseHoldingDown)
			{
				component.SetGameObjectInfo(null, null);
				goto DrawGizmosSection;
			}

			if (string.IsNullOrWhiteSpace(config.brush.property))
				goto DrawGizmosSection;

			GameObject cameraGameObject;

			var camera = FindAnyObjectByType<Camera>();

			if (camera)
			{
				cameraGameObject = camera.gameObject;
			}
			else
			{
				cameraGameObject = new GameObject("Camera");
				cameraGameObject.transform.SetParent(component.transform);
				camera = cameraGameObject.AddComponent<Camera>();
				camera.tag = "MainCamera";
			}

			if (!cameraGameObject.GetComponent<RenderDepthTexture>())
				cameraGameObject.AddComponent<RenderDepthTexture>();

			if (!cameraGameObject.GetComponent<PostProcessLayer>())
				cameraGameObject.AddComponent<PostProcessLayer>();

			if (!Mathf.Approximately(e.delta.x, 0) || !Mathf.Approximately(e.delta.y, 0))
			{
				component.SetGameObjectInfo(gameObject, config.brush.property);

				var point = hit.point;

				if (holdingDown_F)
				{
					component.AddBrushStroke(new()
					{
						points = new() { point, null },
						brush = config.brush,
						fill = true
					});
				}
				else
				{
					//var point2 = point;
					//point2.x -= hit.transform.position.x;
					//point2.x *= -1;
					//point2.x += hit.transform.position.x;

					component.AddBrushStroke(new()
					{
                        //points = new[] { point, point2 },
                        points = new() { point, null },
                        brush = config.brush
					});
				}
			}

		DrawGizmosSection:

			var p = hit.point;
			var n = hit.normal;
			DrawGizmos(p, n);

			// Mirror
			//p.x -= hit.transform.position.x;
			//p.x *= -1;
			//p.x += hit.transform.position.x;
			//n.x *= -1;
			//DrawGizmos(p, n);
		}

		void DrawGizmos(Vector3 p, Vector3 n)
		{
			Handles.color = new Color(.5f, .5f, 1, .4f);

			Handles.DrawWireDisc(p, n, config.brush.size, 2);
			//Handles.DrawLine(p, p + n * .1f, 2);
			//Handles.ArrowHandleCap(0, p + n * .15f, Quaternion.LookRotation(-n), .1f, e.type);

			Handles.color = leftMouseHoldingDown ? new(1, 1, 1, 1) : new(.5f, .5f, 1, .9f);

			var inner = Mathf.Lerp(.1f, .95f, config.brush.hardness);
			Handles.DrawWireDisc(p, n, config.brush.size * inner, Mathf.Lerp(2, 1, config.brush.hardness));

			if (holdingDown_F)
			{
				for (int i = 2; i <= 4; i++)
				{
					Handles.color = new(1, 1, 1, 1);
					Handles.DrawWireDisc(p, n, config.brush.size * i, 1);
				}
			}
		}

		bool TryPickColor(RaycastHit hit, out Color color)
		{
			color = default;

			if (!hit.transform.TryGetComponent<MaterialPropertyOverride.MaterialPropertyOverride>(out var o))
				return false;

			foreach (var prop in o.textureProperties)
			{
				if (prop.name != config.brush.property)
					continue;

				var texture = prop.value as Texture2D;
				if (!texture)
					return false;

				// Only works with a mesh collider (and not with a sphere collider)
				var coord = hit.textureCoord;

				// The recieved color is not correct (it is brighter?!)
				color = texture.GetPixel(
					Mathf.RoundToInt(coord.x * texture.width),
					Mathf.RoundToInt(coord.y * texture.height)
				);
				return true;
			}

			return false;
		}

		//void OnSceneGUI_ColorPickerMode(Event e)
		//{
		//    if (!leftMouseDown)
		//        return;
		//
		//    var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
		//    if (!Physics.Raycast(ray, out var hit))
		//        return;
		//
		//    if (!hit.transform.TryGetComponentInChildren<Renderer>(out var renderer))
		//        return;
		//
		//    var texture = renderer.sharedMaterial.GetTexture(config.brush.property) as Texture2D;
		//    if (!texture || !texture.isReadable)
		//        return;
		//
		//    var coord = hit.textureCoord;
		//    var pixel = texture.GetPixel(
		//        Mathf.RoundToInt(coord.x * texture.width),
		//        Mathf.RoundToInt(coord.y * texture.height)
		//    );
		//
		//    this.Log(pixel);
		//}
	}
}