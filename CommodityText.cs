using System;
using System.Collections.Generic;

namespace ItemCopy
{
    // Pure bilingual inventory copy; no pricing, shapes or firearm mechanics.
    public static class CommodityText
    {
        public sealed class Entry
        {
            public readonly string ZhName, EnName, ZhDescription, EnDescription;
            public Entry(string zh, string en, string zd, string ed) { ZhName=zh; EnName=en; ZhDescription=zd; EnDescription=ed; }
        }
        public static readonly IReadOnlyDictionary<string,Entry> Entries = Build();
        private static Dictionary<string,Entry> Build()
        {
            var d=new Dictionary<string,Entry>(StringComparer.Ordinal);
            Add(d,"earlyfirearms.paper_cartridge","纸包火药和弹丸","Paper Cartridge and Shot","一包扎好的火药与弹丸。纸张已泛黄，收藏者仍能闻出旧时代的硝烟。","A tied packet of powder and shot. The paper has yellowed, but a collector can still catch a whiff of another age.");
            Ammo(d,"ak47prototype.ammo","7.62×39 毫米弹药","7.62×39 mm Ammunition","结实纸盒里装着常见的中间威力弹。它的身影和那支弯弹匣步枪一样，遍布各个角落。","A sturdy carton of familiar intermediate cartridges. Almost as widespread as the curved-magazine rifle they are associated with.");
            Ammo(d,"glock17prototype.ammo","9×19 毫米弹药","9×19 mm Ammunition","一盒9×19毫米帕拉贝鲁姆弹药。包装朴素，却是柜台上很少缺席的熟面孔。","A box of 9×19 mm Parabellum cartridges. Plain packaging, and a familiar sight on almost any weapons counter.");
            Ammo(d,"coldwarmodern.ammo.55645","5.56×45 毫米北约弹药","5.56×45 mm NATO Ammunition","一盒轻巧尖头的步枪弹药。军用标签已经磨损，口径标记依然清晰。","A carton of slender rifle cartridges. The military label is worn, but the caliber marking remains legible.");
            Ammo(d,"coldwarmodern.ammo.76251","7.62×51 毫米北约弹药","7.62×51 mm NATO Ammunition","一批分量扎实的全威力步枪弹。隔着纸盒，也能感到老式军械的沉稳。","A substantial batch of full-power rifle cartridges. Even through the carton, they carry the weight of traditional military hardware.");
            Ammo(d,"coldwarmodern.ammo.5728","5.7×28 毫米弹药","5.7×28 mm Ammunition","小口径的细长弹药，整齐排列在包装里。尺寸特别，挑选买家时也格外讲究。","Slender small-caliber cartridges arranged neatly in their packaging. An unusual size for a more particular buyer.");
            Ammo(d,"coldwarmodern.ammo.44mag",".44 马格南弹药",".44 Magnum Ammunition","一盒黄铜底缘醒目的大口径转轮枪弹。光看包装，就知道它不是温和的收藏。","A box of large revolver cartridges with prominent brass rims. Even the packaging suggests an assertive collection.");
            Ammo(d,"coldwarmodern.ammo.12g","12号霰弹（12/70）","12-Gauge Shells (12/70)","一盒12/70规格霰弹。结实的壳体和清楚的标识，让老派猎具柜显得更完整。","A box of 12/70 shotgun shells. Stout cases and clear markings complete a traditional sporting-gun display.");
            Ammo(d,"coldwarmodern.ammo.50bmg",".50 BMG 大口径弹药",".50 BMG Ammunition","一批尺寸惊人的大口径步枪弹。摆在柜台上时，旁边的商品都会显得小巧几分。","An imposing batch of large rifle cartridges. Everything else on the counter seems smaller beside them.");
            Ammo(d,"local.modernwars.ammo_45colt",".45 柯尔特弹药",".45 Colt Ammunition","带底缘的经典转轮枪弹，透着边疆年代的老派气息。","Classic rimmed revolver cartridges with a distinctly frontier-era character.");
            Ammo(d,"local.modernwars.ammo_45acp",".45 ACP 弹药",".45 ACP Ammunition","一批短而粗壮的手枪弹。朴素可靠的外形，早已成为经典的一部分。","A batch of short, stout pistol cartridges. Their unassuming appearance has long since become part of the classic.");
            Ammo(d,"local.modernwars.ammo_4440",".44-40 温彻斯特弹药",".44-40 WCF Ammunition","一盒带瓶颈的老式弹药。和温彻斯特杠杆机构一样，深得怀旧买家欢心。","A box of old-fashioned bottleneck cartridges. Like the Winchester lever action, they appeal to nostalgic buyers.");
            Ammo(d,"local.modernwars.ammo_76254r","7.62×54R 弹药","7.62×54R Ammunition","底缘突出的长寿军用步枪弹。包装换过许多回，轮廓却始终熟悉。","Long-serving military rifle cartridges with pronounced rims. The packaging has changed many times; the silhouette has not.");
            Ammo(d,"local.modernwars.ammo_79257","7.92×57 毫米毛瑟弹药","7.92×57 mm Mauser Ammunition","一批线条修长的毛瑟步枪弹。老军械收藏里常能找到它们的位置。","A batch of slender Mauser rifle cartridges. There is usually a place for them in an old military collection.");
            Ammo(d,"local.modernwars.ammo_3006",".30-06 斯普林菲尔德弹药",".30-06 Springfield Ammunition","一盒经典军用步枪弹。黄铜与纸板的组合，带着熟悉的军需仓库气息。","A carton of classic military rifle cartridges. Brass and cardboard lend it the familiar character of a quartermaster's store.");
            Ammo(d,"local.modernwars.ammo_76225","7.62×25 毫米托卡列夫弹药","7.62×25 mm Tokarev Ammunition","一批小巧的瓶颈手枪弹。分量不大，却很容易引起老式冲锋枪收藏者的注意。","A compact batch of bottleneck pistol cartridges. A small parcel that readily catches the eye of a vintage submachine-gun collector.");
            Ammo(d,"local.modernwars.ammo_79233","7.92×33 毫米短弹","7.92×33 mm Kurz Ammunition","一盒短弹壳的中间威力弹。短短的外形，见证了军用步枪设计的一次转向。","A box of short-cased intermediate cartridges. Their compact form marks a turning point in military rifle design.");
            Magazine(d,"ak47prototype.magazine","AK 30发弹匣","AK 30-Round Magazine","AK-47","AK-47","弯曲的钢制外壳磨得发亮，仍保留着粗犷的军用线条。","Its curved steel shell is polished by wear, retaining a rugged military outline.");
            Magazine(d,"glock17prototype.magazine","格洛克17弹匣","GLOCK 17 Magazine","格洛克17","GLOCK 17","外形简洁的可拆卸弹匣，没有多余装饰。","A plain detachable magazine with little interest in ornament.");
            Magazine(d,"coldwarmodern.m16a1.feed","M16A1弹匣","M16A1 Magazine","M16A1","M16A1","轻薄的金属壳体与那副细长枪身十分相称。","A light metal shell that complements the rifle's slender frame.");
            Magazine(d,"coldwarmodern.fnfal.feed","FN FAL弹匣","FN FAL Magazine","FN FAL","FN FAL","厚实的方形弹匣，带着全尺寸战斗步枪的分量。","A substantial box magazine with the heft of a full-sized battle rifle.");
            Magazine(d,"coldwarmodern.mp5.feed","MP5弹匣","MP5 Magazine","MP5","MP5","修长的弧形轮廓，是这支冲锋枪的熟悉搭档。","Its slender curved outline is a familiar companion to this submachine gun.");
            Magazine(d,"coldwarmodern.p90.feed","P90弹匣","P90 Magazine","P90","P90","狭长的顶部弹匣，独特造型让它很难与别的配件混淆。","An elongated top-mounted magazine with a shape that is difficult to mistake.");
            Magazine(d,"coldwarmodern.m82a1.feed","巴雷特M82A1弹匣","Barrett M82A1 Magazine","巴雷特M82A1","Barrett M82A1","厚重的外壳像是为大尺寸弹药量身定做的。","Its heavy shell looks made to match the rifle's imposing cartridges.");
            Add(d,"coldwarmodern.m249.feed","M249弹链组件","M249 Belt Assembly","与M249配套的弹链供弹组件。金属环节排列整齐，颇受军械收藏者青睐。","A belt-feed assembly for the M249. Neatly arranged metal links appeal to military-hardware collectors.");
            Add(d,"coldwarmodern.m60.feed","M60弹链组件","M60 Belt Assembly","一套与M60配套的弹链组件。沉甸甸的金属环节，给柜台添了几分前线仓库的气息。","A belt assembly for the M60. Its weighty links lend the counter something of a field armory's atmosphere.");
            Magazine(d,"rimfire22.markiv.magazine","鲁格Mark IV弹匣","Ruger Mark IV Magazine","鲁格Mark IV","Ruger Mark IV","细长而轻巧，和靶射手枪一样朴实利落。","Slender and light, as straightforward as the target pistol it accompanies.");
            Magazine(d,"rimfire22.p22.magazine","瓦尔特P22弹匣","Walther P22 Magazine","瓦尔特P22","Walther P22","小巧的壳体，适合放在紧凑手枪的配件旁。","A compact shell at home beside a small pistol's accessories.");
            Magazine(d,"rimfire22.buckmark.magazine","勃朗宁Buck Mark弹匣","Browning Buck Mark Magazine","勃朗宁Buck Mark","Browning Buck Mark","窄长的外形带着靶场用品惯有的整洁。","Its narrow shape has the tidy appearance of familiar range equipment.");
            Magazine(d,"rimfire22.1022.magazine","鲁格10/22弹匣","Ruger 10/22 Magazine","鲁格10/22","Ruger 10/22","紧凑的供弹部件，正好衬托出步枪的轻巧。","A compact feed component that suits the rifle's light proportions.");
            Magazine(d,"local.modernwars.part_m1911","M1911弹匣","M1911 Magazine","M1911","M1911","平直的金属外壳，有着经典手枪配件的低调。","A straight metal shell with the understated character of a classic pistol accessory.");
            Magazine(d,"local.modernwars.part_thompson1928","汤普森M1928A1弹匣","Thompson M1928A1 Magazine","汤普森M1928A1","Thompson M1928A1","沉稳的长方形外壳，与厚重枪身很是相配。","A solid rectangular shell well matched to the weighty firearm.");
            Magazine(d,"local.modernwars.part_ppsh41","波波沙PPSh-41弹鼓","PPSh-41 Drum Magazine","PPSh-41","PPSh-41","圆鼓鼓的金属壳体格外醒目，是这支冲锋枪的经典轮廓。","Its round metal body forms an unmistakable part of this submachine gun's classic outline.");
            Magazine(d,"local.modernwars.part_bar1918","BAR M1918弹匣","BAR M1918 Magazine","BAR M1918","BAR M1918","结实的方形弹匣，军用磨痕比装饰更显眼。","A stout box magazine whose service wear stands out more than any decoration.");
            Magazine(d,"local.modernwars.part_stg44","StG 44弹匣","StG 44 Magazine","StG 44","StG 44","长而弯曲的金属轮廓，带着早期突击步枪的独特气质。","A long curved metal profile with the distinctive character of an early assault rifle.");
            Add(d,"local.modernwars.clip_mosin1891","莫辛五发桥夹","Mosin Five-Round Stripper Clip","一枚莫辛步枪的五发桥夹。薄薄一片金属，也能成为老军械收藏里不可少的细节。","A five-round stripper clip for a Mosin rifle. A thin piece of metal, but a telling detail in a vintage collection.");
            Add(d,"local.modernwars.clip_kar98k","毛瑟五发桥夹","Mauser Five-Round Stripper Clip","一枚毛瑟步枪的五发桥夹。朴素的小零件，和栓动步枪一样经得住岁月。","A five-round Mauser stripper clip. A modest accessory with the same enduring character as a bolt-action rifle.");
            Add(d,"local.modernwars.clip_m1garand","M1加兰德八发整装弹夹","M1 Garand Eight-Round En Bloc Clip","一枚M1加兰德的八发整装弹夹。外形简单，在收藏者眼里却有独特的声响与故事。","An eight-round en bloc clip for the M1 Garand. Simple in shape, yet rich in familiar sounds and stories for a collector.");
            Add(d,"local.modernwars.belt_mg42","MG 42弹链","MG 42 Ammunition Belt","一条供MG 42使用的弹链。连续的金属环节，为收藏柜添上沉重的军械细节。","An ammunition belt for the MG 42. A line of metal links adding a weighty military detail to a collection.");
            return d;
        }
        private static void Add(Dictionary<string,Entry> d,string id,string zh,string en,string zd,string ed) { d.Add(id,new Entry(zh,en,zd,ed)); }
        private static void Ammo(Dictionary<string,Entry> d,string id,string zh,string en,string zd,string ed) { Add(d,id,zh,en,zd,en+". "+ed); }
        private static void Magazine(Dictionary<string,Entry> d,string id,string zh,string en,string gz,string ge,string zd,string ed) { Add(d,id,zh,en,"适配"+gz+"的弹匣。"+zd,"A magazine for the "+ge+". "+ed); }
        public static string Name(string id,bool english) { var e=Entries[id];return english?e.EnName:e.ZhName; }
        public static string Description(string id,bool english) { var e=Entries[id];return english?e.EnDescription:e.ZhDescription; }
    }
}
