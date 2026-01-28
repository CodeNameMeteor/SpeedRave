using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;
using System.IO; 

namespace SpeedRave
{
    public class InventoryOverlay : MonoBehaviour
    {
        // Settings
        public static bool showInventory;
        public static bool useIcons;
        public static bool verticalIcons;

        public static float iconSize;
        public static float padding;
        public static float textHeight;
        public static float rowHeight = 55;

        // Flags
        private bool fontFound = false;
        private bool texturesResolved = false;

        // References to objects I should figure out a cleaner way to do this
        private GameObject inventoryGO;
        private FoodControl foodControl;

        // Textures
        private Texture2D cheeseTexture; 
        private Texture2D fruitTexture;

        // Path to textures
        private string texturePath;

        // Data Structures
        private class ItemDef
        {
            public string boolFieldName;
            public string objectFieldName;

            public Texture texture;
            public Rect uvRect;

            public bool isCollected;
        }

        private List<ItemDef> allItems = new List<ItemDef>();
        private List<Texture> displayList = new List<Texture>();
        private List<Rect> displayUVs = new List<Rect>();

        // Styles
        private GUIStyle textStyle;
        private bool initialized = false;

        private void Awake()
        {
            textStyle = new GUIStyle();
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = (int)textHeight;
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.richText = true;


            // Texture path: Sewer Rave/BepInEx/CustomTextures
            texturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "CustomTextures");
        }

        private void Start()
        {

            allItems.Add(new ItemDef { boolFieldName = "haveKey", objectFieldName = "key" });
            allItems.Add(new ItemDef { boolFieldName = "hasDuck", objectFieldName = "ducky" });
            allItems.Add(new ItemDef { boolFieldName = "hasPizza", objectFieldName = "pizza" });
            allItems.Add(new ItemDef { boolFieldName = "hasMug", objectFieldName = "mug" });
            allItems.Add(new ItemDef { boolFieldName = "hasPyramid", objectFieldName = "pyramid" });
            allItems.Add(new ItemDef { boolFieldName = "hasBottlecap", objectFieldName = "bottlecap" });
        }

        private void Update()
        {
            //if (!fontFound) AttemptFindFont();

            // Try to initialize references
            if (!initialized || inventoryGO == null)
            {
                AttemptInit();
            }

            // Retry loading local textures if they failed initially (optional fallback)
            /*
            if (useIcons && !texturesResolved)
            {
                LoadLocalTextures();
            }
            */

            if (initialized) CheckInventoryState();

            if (initialized && !fontFound) AttemptFindFont();
        }

        private void LoadLocalTextures()
        {
            if (cheeseTexture == null) cheeseTexture = LoadTextureFromFile("cheese.png");
            if (fruitTexture == null) fruitTexture = LoadTextureFromFile("fruit.png");

            if (cheeseTexture != null && fruitTexture != null)
            {
                texturesResolved = true;
            }
        }

        private Texture2D LoadTextureFromFile(string filename)
        {
            string fullPath = Path.Combine(texturePath, filename);

            if (File.Exists(fullPath))
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(fullPath);
                    Texture2D tex = new Texture2D(2, 2); // Size doesn't matter, LoadImage replaces it
                    if (tex.LoadImage(fileData))
                    {
                        tex.name = filename;
                        tex.filterMode = FilterMode.Bilinear; // Makes scaling smoother
                        return tex;
                    }
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
            Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();

            foreach (Font font in allFonts)
            {
                if (font == null) continue;
                string fName = font.name.ToLower();
                if (fName.Contains("autumn") || fName.Contains("larua"))
                {
                    textStyle.font = font;
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

                // Attempt to load local textures immediately
                LoadLocalTextures();

                // Find Item Textures (Keep existing logic for game items)
                foreach (var item in allItems)
                {
                    if (item.texture != null) continue;

                    FieldInfo objField = typeof(FoodControl).GetField(item.objectFieldName);
                    if (objField != null)
                    {
                        GameObject itemGO = objField.GetValue(foodControl) as GameObject;
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
                FieldInfo boolField = typeof(FoodControl).GetField(item.boolFieldName);
                if (boolField != null)
                {
                    bool hasItem = (bool)boolField.GetValue(foodControl);

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
            if (!showInventory || !initialized || foodControl == null || foodControl.display || textStyle == null) return;

            // 1. Setup dynamic styling
            textStyle.fontSize = (int)textHeight;
            float startX = 10f; // Added a small margin from the left edge

            // 2. Calculate the base Y (starting from the bottom)
            // We subtract iconSize to ensure the bottom-most icon stays on screen
            float currentY = Screen.height - iconSize ;

            bool canShowLogos = useIcons && cheeseTexture != null && fruitTexture != null;

            // 3. Draw Cheese and Fruit
            if (canShowLogos)
            {
                // Draw Fruit (Bottom row)
                DrawRow(startX, currentY, fruitTexture, new Rect(0, 0, 1, 1), foodControl.fruit.ToString());

                // Move Y up for the Cheese row
                currentY -= (iconSize + padding);

                // Draw Cheese (Next row up)
                DrawRow(startX, currentY, cheeseTexture, new Rect(0, 0, 1, 1), foodControl.cheese.ToString());

                // Move Y up again to start drawing the Item list
                currentY -= (iconSize + padding);
            }
            else
            {
                // Text-only fallback
                string txt = $"Cheese: {foodControl.cheese}    Fruit: {foodControl.fruit}";
                DrawTextWithShadow(startX, currentY, txt);
                currentY -= textHeight + padding;
            }

            // 4. Draw Items (Collected Items)
            float itemX = startX;
            for (int i = 0; i < displayList.Count; i++)
            {
                Texture tex = displayList[i];
                Rect uv = displayUVs[i];

                if (tex != null)
                {
                    DrawIconWithShadow(itemX, currentY, iconSize, tex, uv);

                    if (verticalIcons)
                    {
                        // Stack upwards
                        currentY -= (iconSize + padding);
                    }
                    else
                    {
                        // Grid rightwards
                        itemX += (iconSize + padding);
                    }
                }
            }
        }

        private void DrawRow(float x, float y, Texture icon, Rect uv, string countText)
        {
            DrawIconWithShadow(x, y, iconSize, icon, uv);

            float textX = x + iconSize + 10f;
            float textY = y + (iconSize / 2f) - (textHeight / 2f);

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
            if (textStyle == null) return;

            GUIStyle shadowStyle = new GUIStyle(textStyle);
            shadowStyle.normal.textColor = Color.black;
            shadowStyle.font = textStyle.font;

            GUI.Label(new Rect(x + 2, y + 2, 200, textHeight), content, shadowStyle);
            GUI.Label(new Rect(x, y, 200, textHeight), content, textStyle);
        }
    }
}