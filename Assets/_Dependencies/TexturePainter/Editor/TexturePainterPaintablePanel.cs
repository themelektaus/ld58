using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TexturePainter.Editor
{
	public class TexturePainterPaintablePanel : VisualElement
	{
		GameObject gameObject;
		Paintable paintable;
		string propertyName;

		Button registerPaintableButton;
		Button createTextureButton;
		Button unregisterPaintableButton;

		// Custom magic method
		void OnCreate()
		{
			registerPaintableButton = this.Q<Button>("RegisterPaintableButton");
			registerPaintableButton.clicked += () =>
			{
				var component = Utils.GetTexturePainterComponent();
				component.CreateDataObject(gameObject);
				paintable = component.Register(gameObject);
				Refresh();
			};

			createTextureButton = this.Q<Button>("CreateTextureButton");
			createTextureButton.clicked += () =>
			{
				var component = Utils.GetTexturePainterComponent();
				if (component.CreateTexture(gameObject, propertyName, 1024, Color.gray))
					Refresh();
			};

			unregisterPaintableButton = this.Q<Button>("UnregisterPaintableButton");
			unregisterPaintableButton.clicked += () =>
			{
				var component = Utils.GetTexturePainterComponent();
				if (component.Unregister(gameObject) is null)
					return;

				paintable = null;
				Refresh();
			};
		}

		public void Select(GameObject gameObject, Paintable paintable)
		{
			if (gameObject)
				gameObject = gameObject.GetGameObjectWithMaterial();

			if (this.gameObject == gameObject && this.paintable == paintable)
				return;

			this.gameObject = gameObject;
			this.paintable = paintable;

			Refresh();
		}

		public void Select(string propertyName)
		{
			if (this.propertyName == propertyName)
				return;

			this.propertyName = propertyName;

			Refresh();
		}

		public void Unselect()
		{
			if (gameObject is null && paintable is null && propertyName is null)
				return;

			gameObject = null;
			paintable = null;
			propertyName = null;
			Refresh();
		}

		public void Refresh()
		{
			var content = this.Q("Content");

			content.Clear();

			if (paintable is null)
			{
				registerPaintableButton.SetEnabled(gameObject);
				createTextureButton.SetEnabled(false);
				unregisterPaintableButton.SetEnabled(false);
				return;
			}

			registerPaintableButton.SetEnabled(false);
			createTextureButton.SetEnabled(true);

			foreach (var textureInfo in paintable.textureInfos)
			{
				var element = new VisualElement
				{
					style = { flexDirection = FlexDirection.Row }
				};

				if (propertyName == textureInfo.propertyName)
					createTextureButton.SetEnabled(false);

				var propertyNameField = new TextField
				{
					value = textureInfo.propertyName,
					style = { width = 120 }
				};
				propertyNameField.RegisterValueChangedCallback(x =>
				{
					textureInfo.propertyName = x.newValue;
				});
				element.Add(propertyNameField);

				var textureField = new ObjectField
				{
					value = textureInfo.texture,
					objectType = typeof(Texture2D),
					allowSceneObjects = false,
					style = { flexBasis = 0, flexGrow = 1 }
				};
				textureField.RegisterValueChangedCallback(x =>
				{
					textureInfo.texture = x.newValue as Texture2D;
				});
				element.Add(textureField);

				var deleteButton = new Button(() =>
				{
					var component = Utils.GetTexturePainterComponent();
					component.GetGameObjectInfo(gameObject).UnsetTexture(textureInfo.propertyName);
					paintable.RemoveTexture(textureInfo.propertyName);
					Refresh();
				})
				{
					text = "Delete",
					focusable = false
				};
				element.Add(deleteButton);

				content.Add(element);
			}

			unregisterPaintableButton.SetEnabled(createTextureButton.enabledSelf);
		}
	}
}