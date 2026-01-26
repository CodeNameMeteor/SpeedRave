using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

namespace SpeedRave
{
    public class ClearInventory : MonoBehaviour
    {
        // --- SETTINGS ---
        public bool showInventory = true;
        public bool useIcons = true; // Toggle for Cheese/Fruit logos

        private float iconSize = 50f;
        private float padding = 10f;
        private float textHeight = 45f;
        private float rowHeight = 55f;

        // Internal Logic Flags
        private bool fontFound = false;
        private bool texturesResolved = false;
        private float scanTimer = 0f;

        // References
        private GameObject inventoryGO;
        private FoodControl foodControl;

        // Textures
        private Texture cheeseTexture;
        private Texture fruitTexture;

        // Data Structures
        private class ItemDef
        {
            public string boolFieldName;
            public string objectFieldName;

            public Texture texture;
            public Rect uvRect; // Defines which part of the texture to draw (0-1)

            public bool isCollected;
        }

        private List<ItemDef> allItems = new List<ItemDef>();
        private List<Texture> displayList = new List<Texture>();
        // Helper list to store UVs for the display list items
        private List<Rect> displayUVs = new List<Rect>();

        // Styles
        private GUIStyle textStyle;
        private bool initialized = false;

        private void Awake()
        {
            textStyle = new GUIStyle();
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 30;
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.richText = true;
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
            if (!fontFound) AttemptFindFont();

            if (!initialized || inventoryGO == null)
            {
                AttemptInit();
            }
            // Lazy Scanner: Only run if we WANT icons but don't have them yet
            else if (useIcons && !texturesResolved)
            {
                scanTimer += Time.deltaTime;
                if (scanTimer > 3.0f)
                {
                    AttemptReFindTextures();
                    scanTimer = 0f;
                }
            }

            if (initialized) CheckInventoryState();
        }

        private void AttemptReFindTextures()
        {
            if (cheeseTexture == null) cheeseTexture = FindTextureExact("cheese");
            if (fruitTexture == null) fruitTexture = FindTextureExact("fruit");

            if (cheeseTexture != null && fruitTexture != null)
            {
                texturesResolved = true;
            }
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
            inventoryGO = GameObject.FindGameObjectWithTag("Inventory");

            if (inventoryGO != null)
            {
                foodControl = inventoryGO.GetComponent<FoodControl>();

                // Attempt to find textures immediately
                AttemptReFindTextures();

                // Find Item Textures
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

        // Updated extractor: Gets Texture AND UV Rect (for spritesheets)
        private void ExtractTextureInfo(GameObject go, ItemDef item)
        {
            Sprite sprite = null;

            // Check UI Image
            var img = go.GetComponent<Image>();
            if (img != null) sprite = img.sprite;

            // Check SpriteRenderer
            if (sprite == null)
            {
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) sprite = sr.sprite;
            }

            // Check Children
            if (sprite == null)
            {
                var childImg = go.GetComponentInChildren<Image>();
                if (childImg != null) sprite = childImg.sprite;
            }

            if (sprite != null)
            {
                item.texture = sprite.texture;

                // --- ATLAS CALCULATION ---
                // Calculate normalized UV coordinates based on where the sprite is in the texture
                Rect r = sprite.textureRect;
                float w = sprite.texture.width;
                float h = sprite.texture.height;

                // Create UV rect (x, y, width, height) in 0-1 range
                item.uvRect = new Rect(r.x / w, r.y / h, r.width / w, r.height / h);
            }
            else
            {
                // Fallback (full texture)
                item.uvRect = new Rect(0, 0, 1, 1);
            }
        }

        private Texture FindTextureExact(string exactName)
        {
            Texture[] allTex = Resources.FindObjectsOfTypeAll<Texture>();
            foreach (Texture t in allTex)
            {
                if (t.name.Equals(exactName, StringComparison.OrdinalIgnoreCase))
                    return t;
            }
            return null;
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

            float startX = 20f;
            float bottomY = Screen.height - 50f;

            // --- 1. DRAW CHEESE / FRUIT ---

            // Logic: Do we have textures AND is Logo Mode on?
            bool canShowLogos = useIcons && cheeseTexture != null && fruitTexture != null;

            if (canShowLogos)
            {
                // LOGO MODE
                float fruitY = bottomY;
                DrawRow(startX, fruitY, fruitTexture, new Rect(0, 0, 1, 1), foodControl.fruit.ToString());

                float cheeseY = fruitY - rowHeight;
                DrawRow(startX, cheeseY, cheeseTexture, new Rect(0, 0, 1, 1), foodControl.cheese.ToString());
            }
            else
            {
                // TEXT MODE (Fallback)
                // We draw the text string at the bottom
                string txt = $"Cheese: {foodControl.cheese}   Fruit: {foodControl.fruit}";

                // Shadow
                GUIStyle shadow = new GUIStyle(textStyle);
                shadow.normal.textColor = Color.black;
                shadow.font = textStyle.font;

                GUI.Label(new Rect(startX + 2, bottomY + 2, 400, textHeight), txt, shadow);
                GUI.Label(new Rect(startX, bottomY, 400, textHeight), txt, textStyle);
            }

            // --- 2. DRAW ITEMS ---

            // Calculate where items start based on what we drew below
            float itemsStartY = bottomY;
            if (canShowLogos)
            {
                // Items go above Cheese (which is at bottomY - rowHeight)
                itemsStartY = (bottomY - rowHeight) - rowHeight;
            }
            else
            {
                // Items go just above the single text line
                itemsStartY = bottomY - rowHeight;
            }

            float currentX = startX;

            for (int i = 0; i < displayList.Count; i++)
            {
                Texture tex = displayList[i];
                Rect uv = displayUVs[i];

                if (tex != null)
                {
                    DrawIconWithShadow(currentX, itemsStartY, iconSize, tex, uv);
                    currentX += iconSize + padding;
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
            // Calculate screen rects
            Rect shadowRect = new Rect(x + 2, y + 2, size, size);
            Rect mainRect = new Rect(x, y, size, size);

            // Shadow
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTextureWithTexCoords(shadowRect, tex, uv);

            // Main
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