using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpeedRave
{
    public class InventoryOverlay : MonoBehaviour
    {
        // Flags
        public static Font GameFont { get; private set; }
        private bool fontFound = false;
        private float lastFontSearchTime = -10f;
        private const float FontSearchCooldown = 5f;

        // References
        private GameObject inventoryGO;
        private FoodControl foodControl;

        // Textures
        private Texture2D cheeseTexture; 
        private Texture2D fruitTexture;
        private string texturePath;

        // Data Structures
        private class ItemDef
        {
            public string boolFieldName;
            public string objectFieldName;
            public FieldInfo boolField;
            public FieldInfo objectField;

            public Texture texture;
            public Rect uvRect;
            public bool isCollected;
        }

        private readonly List<ItemDef> allItems = new List<ItemDef>();
        private readonly List<Texture> displayList = new List<Texture>();
        private readonly List<Rect> displayUVs = new List<Rect>();

        // Styles
        private GUIStyle textStyle;
        private GUIStyle shadowStyle;
        private bool initialized = false;

        private void Awake()
        {
            textStyle = new GUIStyle();
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = (int)Plugin.TextHeight.Value;
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.richText = true;

            shadowStyle = new GUIStyle();
            shadowStyle.normal.textColor = Color.black;
            shadowStyle.fontSize = (int)Plugin.TextHeight.Value;
            shadowStyle.alignment = TextAnchor.MiddleLeft;
            shadowStyle.richText = true;

            texturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "CustomTextures");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (cheeseTexture != null) Destroy(cheeseTexture);
            if (fruitTexture != null) Destroy(fruitTexture);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            initialized = false;
            inventoryGO = null;
            foodControl = null;
        }

        private void Start()
        {
            AddItemDef("haveKey", "key");
            AddItemDef("hasDuck", "ducky");
            AddItemDef("hasPizza", "pizza");
            AddItemDef("hasMug", "mug");
            AddItemDef("hasPyramid", "pyramid");
            AddItemDef("hasBottlecap", "bottlecap");

            AttemptFindFont();
        }

        private void AddItemDef(string boolField, string objField)
        {
            allItems.Add(new ItemDef
            {
                boolFieldName = boolField,
                objectFieldName = objField,
                boolField = typeof(FoodControl).GetField(boolField),
                objectField = typeof(FoodControl).GetField(objField)
            });
        }

        private void Update()
        {
            if (!initialized || inventoryGO == null)
            {
                AttemptInit();
            }

            if (initialized)
            {
                CheckInventoryState();

                if (!fontFound && Time.unscaledTime - lastFontSearchTime > FontSearchCooldown)
                {
                    AttemptFindFont();
                }
            }
        }

        private void LoadLocalTextures()
        {
            if (cheeseTexture == null) cheeseTexture = LoadTextureFromFile("cheese.png");
            if (fruitTexture == null) fruitTexture = LoadTextureFromFile("fruit.png");
        }

        private Texture2D LoadTextureFromFile(string filename)
        {
            string fullPath = Path.Combine(texturePath, filename);

            if (File.Exists(fullPath))
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(fullPath);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(fileData))
                    {
                        tex.name = filename;
                        tex.filterMode = FilterMode.Bilinear;
                        return tex;
                    }
                    Destroy(tex);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SpeedRave] Failed to load texture {filename}: {e.Message}");
                }
            }
            return null;
        }

        private void AttemptFindFont()
        {
            lastFontSearchTime = Time.unscaledTime;
            Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();

            foreach (Font font in allFonts)
            {
                if (font == null) continue;
                string fName = font.name.ToLower();
                if (fName.Contains("autumn") || fName.Contains("larua"))
                {
                    textStyle.font = font;
                    shadowStyle.font = font;
                    GameFont = font;
                    fontFound = true;
                    break;
                }
            }
        }

        private void AttemptInit()
        {
            inventoryGO = ReferenceManager.ActiveInventory;

            if (inventoryGO != null)
            {
                foodControl = ReferenceManager.ActiveFoodControl;

                LoadLocalTextures();

                // Find Item Textures using cached FieldInfo
                foreach (var item in allItems)
                {
                    if (item.texture != null) continue;

                    if (item.objectField != null)
                    {
                        GameObject itemGO = item.objectField.GetValue(foodControl) as GameObject;
                        if (itemGO != null)
                        {
                            ExtractTextureInfo(itemGO, item);
                        }
                    }
                }
                initialized = true;
            }
        }

        private void ExtractTextureInfo(GameObject go, ItemDef item)
        {
            Sprite sprite = null;

            var img = go.GetComponent<Image>();
            if (img != null) sprite = img.sprite;

            if (sprite == null)
            {
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) sprite = sr.sprite;
            }

            if (sprite == null)
            {
                var childImg = go.GetComponentInChildren<Image>();
                if (childImg != null) sprite = childImg.sprite;
            }

            if (sprite != null)
            {
                item.texture = sprite.texture;
                Rect r = sprite.textureRect;
                float w = sprite.texture.width;
                float h = sprite.texture.height;
                item.uvRect = new Rect(r.x / w, r.y / h, r.width / w, r.height / h);
            }
            else
            {
                item.uvRect = new Rect(0, 0, 1, 1);
            }
        }

        private void CheckInventoryState()
        {
            if (foodControl == null) return;

            foreach (var item in allItems)
            {
                if (item.boolField != null)
                {
                    bool hasItem = (bool)item.boolField.GetValue(foodControl);

                    if (hasItem && !item.isCollected)
                    {
                        item.isCollected = true;
                        if (item.texture != null)
                        {
                            displayList.Add(item.texture);
                            displayUVs.Add(item.uvRect);
                        }
                    }
                    else if (!hasItem && item.isCollected)
                    {
                        item.isCollected = false;
                        if (item.texture != null)
                        {
                            int index = displayList.IndexOf(item.texture);
                            if (index != -1)
                            {
                                displayList.RemoveAt(index);
                                displayUVs.RemoveAt(index);
                            }
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!Plugin.InventoryOverlayEnabled.Value || !initialized || foodControl == null || foodControl.display || textStyle == null) return;

            int targetFontSize = (int)Plugin.TextHeight.Value;
            textStyle.fontSize = targetFontSize;
            shadowStyle.fontSize = targetFontSize;

            float startX = 10f;
            float currentY = Screen.height - Plugin.IconSize.Value;

            bool canShowLogos = Plugin.UseIcons.Value && cheeseTexture != null && fruitTexture != null;

            if (canShowLogos)
            {
                // Draw Fruit (Bottom row)
                DrawRow(startX, currentY, fruitTexture, new Rect(0, 0, 1, 1), foodControl.fruit.ToString());

                // Move Y up for the Cheese row
                currentY -= (Plugin.IconSize.Value + Plugin.Padding.Value);

                DrawRow(startX, currentY, cheeseTexture, new Rect(0, 0, 1, 1), foodControl.cheese.ToString());

                currentY -= (Plugin.IconSize.Value + Plugin.Padding.Value);
            }
            else
            {
                // Text-only fallback
                string txt = $"Cheese: {foodControl.cheese}    Fruit: {foodControl.fruit}";
                DrawTextWithShadow(startX, currentY, txt);
                currentY -= Plugin.TextHeight.Value + Plugin.Padding.Value;
            }

            float itemX = startX;
            for (int i = 0; i < displayList.Count; i++)
            {
                Texture tex = displayList[i];
                Rect uv = displayUVs[i];

                if (tex != null)
                {
                    DrawIconWithShadow(itemX, currentY, Plugin.IconSize.Value, tex, uv);

                    if (Plugin.VerticalIcons.Value)
                    {
                        currentY -= (Plugin.IconSize.Value + Plugin.Padding.Value);
                    }
                    else
                    {
                        itemX += (Plugin.IconSize.Value + Plugin.Padding.Value);
                    }
                }
            }
        }

        private void DrawRow(float x, float y, Texture icon, Rect uv, string countText)
        {
            DrawIconWithShadow(x, y, Plugin.IconSize.Value, icon, uv);

            float textX = x + Plugin.IconSize.Value + 10f;
            float textY = y + (Plugin.IconSize.Value / 2f) - (Plugin.TextHeight.Value / 2f);

            DrawTextWithShadow(textX, textY, countText);
        }

        private void DrawIconWithShadow(float x, float y, float size, Texture tex, Rect uv)
        {
            Rect shadowRect = new Rect(x + 2, y + 2, size, size);
            Rect mainRect = new Rect(x, y, size, size);

            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTextureWithTexCoords(shadowRect, tex, uv);

            GUI.color = Color.white;
            GUI.DrawTextureWithTexCoords(mainRect, tex, uv);
        }

        private void DrawTextWithShadow(float x, float y, string content)
        {
            if (textStyle == null || shadowStyle == null) return;

            GUI.Label(new Rect(x + 2, y + 2, 200, Plugin.TextHeight.Value), content, shadowStyle);
            GUI.Label(new Rect(x, y, 200, Plugin.TextHeight.Value), content, textStyle);
        }
    }
}