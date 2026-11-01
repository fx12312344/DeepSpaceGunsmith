using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace AmmoMachineTest
{
    // A fixed-height masked viewport using the game's own UI nodes and buttons.
    // No storage nodes: changing scroll position cannot alter the save graph.
    public sealed class AmmoRecipeList : PixelElement
    {
        private readonly TreeNodeRender clip, content, track, thumb;
        private float dragOffset;
        private readonly MachineryButton[] buttons;
        private readonly Action<int> select;
        private int scroll, selected;
        private readonly int viewWidth, viewHeight, rowHeight;
        public bool canResize=>false;
        public int widthPixels=>viewWidth;
        public int heightPixels=>viewHeight;
        public TreeNodeRender handler {get; private set;}
        public RectTransform rectTransform=>handler.rectTransform;
        public TreeNodeRender inner=>clip;
        public AmmoRecipeList(Action<int> onSelect,int width=150,int height=180,int rowHeight=18)
        {
            select=onSelect; viewWidth=width; viewHeight=height; this.rowHeight=rowHeight;
            handler=TreeNodeRender.Instantiate(this,"CNC Recipe List");
            clip=TreeNodeRender.Instantiate().SetParent(handler); clip.mask.enabled=true;
            clip.image.enabled=true; clip.image.color=new Color(0,0,0,0.01f); clip.InitEventHandler();
            clip.eventHandler.mouseScrollFunc.Add(delegate(PointerEventData e){Scroll(e.scrollDelta.y>0?-3:3);return true;});
            content=TreeNodeRender.Instantiate().SetParent(clip); content.image.enabled=false;
            track=TreeNodeRender.Instantiate().SetParent(handler); thumb=TreeNodeRender.Instantiate().SetParent(handler);
            track.image.sprite=RenderHandler.LoadFromAtlas("Window/elements_outer","border_right_scrollbar"); track.image.enabled=true; track.InitEventHandler();
            thumb.image.sprite=RenderHandler.LoadFromAtlas("Window/elements_outer","border_right_scrollbar_button"); thumb.image.enabled=true; thumb.InitEventHandler();
            track.eventHandler.mouseDownFunc.Add(delegate(PointerEventData e){SetScrollFromPointer(e.position,0);return true;});
            track.eventHandler.mouseDragFunc.Add(delegate(PointerEventData e){SetScrollFromPointer(e.position,0);return true;});
            thumb.eventHandler.mouseDownFunc.Add(delegate(PointerEventData e){dragOffset=handler.rectTransform.InverseTransformPoint(e.position).y-thumb.rectTransform.anchoredPosition.y;return true;});
            thumb.eventHandler.mouseDragFunc.Add(delegate(PointerEventData e){SetScrollFromPointer(e.position,dragOffset);return true;});
            buttons=new MachineryButton[AmmoRecipes.All.Length];
            for(int i=0;i<buttons.Length;i++)
            {
                int index=i;
                var b=new MachineryButton((string.Equals(LocHelper.GetCurrentLocaleCode(),"en",StringComparison.OrdinalIgnoreCase) ? AmmoRecipes.All[i].En : AmmoRecipes.All[i].Zh),()=>{selected=index;UpdateSelected();select?.Invoke(index);},width-16,rowHeight-2);
                b.handler.SetParent(content);
                // Forward wheel while hovering a button; its click remains independent.
                var nodes=b.handler.GetComponentsInChildren<TreeNodeRender>(true);
                foreach(var node in nodes) if(node.eventHandler!=null) node.eventHandler.mouseScrollFunc.Add(delegate(PointerEventData e){Scroll(e.scrollDelta.y>0?-3:3);return true;});
                buttons[i]=b;
            }
            UpdateSelected();Validate();
        }
        private void UpdateSelected()
        {
            for(int i=0;i<buttons.Length;i++)buttons[i].SetText((i==selected?"> ":"")+(string.Equals(LocHelper.GetCurrentLocaleCode(),"en",StringComparison.OrdinalIgnoreCase) ? AmmoRecipes.All[i].En : AmmoRecipes.All[i].Zh));
        }
        private void Scroll(int rows)
        {
            int max=Math.Max(0,buttons.Length*rowHeight-viewHeight);
            scroll=Math.Max(0,Math.Min(max,scroll+rows*rowHeight));
            content.rectTransform.anchoredPosition=new Vector2(0,scroll);
            UpdateThumb();
        }
        private int MaxScroll=>Math.Max(0,buttons.Length*rowHeight-viewHeight);
        private int ThumbHeight=>Math.Max(12,viewHeight*viewHeight/Math.Max(viewHeight,buttons.Length*rowHeight));
        private void UpdateThumb()
        {
            int travel=Math.Max(1,viewHeight-ThumbHeight);
            int y=MaxScroll==0?0:Mathf.RoundToInt((float)scroll/MaxScroll*travel);
            thumb.rectTransform.anchoredPosition=new Vector2(viewWidth-8,-y);
        }
        private void SetScrollFromPointer(Vector2 position,float offset)
        {
            float y=handler.rectTransform.InverseTransformPoint(position).y-offset;
            int travel=Math.Max(1,viewHeight-ThumbHeight);
            scroll=Mathf.RoundToInt(Mathf.Clamp01(-y/travel)*MaxScroll);
            content.rectTransform.anchoredPosition=new Vector2(0,scroll);
            UpdateThumb();
        }
        public void Validate()
        {
            handler.rectTransform.sizeDelta=new Vector2(viewWidth,viewHeight);
            clip.rectTransform.sizeDelta=new Vector2(viewWidth-12,viewHeight);
            track.rectTransform.anchoredPosition=new Vector2(viewWidth-8,0);
            track.rectTransform.sizeDelta=new Vector2(8,viewHeight);
            thumb.rectTransform.sizeDelta=new Vector2(8,ThumbHeight);
            UpdateThumb();
            content.rectTransform.sizeDelta=new Vector2(viewWidth-12,buttons.Length*rowHeight);
            content.rectTransform.anchoredPosition=new Vector2(0,scroll);
            for(int i=0;i<buttons.Length;i++)buttons[i].handler.rectTransform.anchoredPosition=new Vector2(0,-i*rowHeight);
        }
        public void ResizePixels(int width,int height)=>Validate();
        public int WidthPixelsClamp(int width)=>viewWidth;
        public int HeightPixelsClamp(int height)=>viewHeight;
        public int XClamp(int x)=>0;
        public int YClamp(int y)=>0;
        public bool Attach(PixelElement element)=>false;
        public bool Detach(PixelElement element)=>false;
    }
}
