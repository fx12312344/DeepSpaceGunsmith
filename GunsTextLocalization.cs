using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeaponDescriptions
{
    // Pure text catalog: no game, Unity, locale, factory or balance dependencies.
    // Keep each firearm's two names and two descriptions together for coverage checks.
    public static class GunsTextLocalization
    {
        public sealed class Entry
        {
            public string ZhName { get; }
            public string EnName { get; }
            public string ZhDescription { get; }
            public string EnDescription { get; }
            public Entry(string zhName, string enName, string zhDescription, string enDescription)
            { ZhName = zhName; EnName = enName; ZhDescription = zhDescription; EnDescription = enDescription; }
        }

        public static IReadOnlyDictionary<string, Entry> Entries { get; } = new ReadOnlyDictionary<string, Entry>(
            new Dictionary<string, Entry>(StringComparer.Ordinal)
            {
                ["hand_cannon"] = new Entry("中世纪手铳（古董）", "Medieval Hand Cannon (Antique)",
                    "一支早期手铳，金属管配上木柄，便有了火器最朴素的模样。它的年代比准头更值得称道。",
                    "An early hand cannon: a metal tube, a wooden handle, and very little refinement. Its age is more impressive than its accuracy."),
                ["matchlock"] = new Entry("荷兰火绳枪（古董）", "Dutch Matchlock Musket (Antique)",
                    "一支荷兰火绳枪。修长的枪管与厚重木托保留着早期军械的庄重，连收藏柜都得为它多让些地方。",
                    "A Dutch matchlock musket with a long barrel and a substantial wooden stock. Even a collector's cabinet needs to make room for this much history."),
                ["wheellock"] = new Entry("荷兰簧轮手枪（古董）", "Dutch Wheellock Pistol (Antique)",
                    "一把荷兰簧轮手枪。精巧的点火机构藏在古朴轮廓之中，足以让懂行的收藏家停下脚步。",
                    "A Dutch wheellock pistol with intricate old-world engineering beneath its antique profile. The sort of piece that makes a knowledgeable collector stop and look."),
                ["blunderbuss"] = new Entry("燧发喇叭口短铳（古董）", "Flintlock Blunderbuss (Antique)",
                    "一支枪口外扩的燧发短铳。短粗的轮廓带着旅人与水手的旧日气息，摆在柜台上也很难忽视。",
                    "A flintlock blunderbuss with a broad, flared muzzle. Its stout silhouette recalls travelers and sailors, and is difficult to overlook on a shop counter."),
                ["brown_bess"] = new Entry("布朗·贝斯燧发火枪（古董）", "Brown Bess Flintlock Musket (Antique)",
                    "一支布朗·贝斯燧发火枪。修长木托与朴实铁件见证了旧式步兵的时代，比亮闪闪的新货更有故事。",
                    "A Brown Bess flintlock musket. Its long stock and plain ironwork recall an age of line infantry, with more stories to tell than a polished new arrival."),
                ["charleville"] = new Entry("夏勒维尔1766式燧发火枪（古董）", "Charleville Model 1766 Flintlock Musket (Antique)",
                    "一支法国夏勒维尔1766式燧发火枪。年代久远，制式步兵军械的利落线条仍清晰可辨。",
                    "A French Charleville Model 1766 flintlock musket. The years have not obscured the purposeful lines of a traditional infantry arm."),
                ["naval_pistol"] = new Entry("海军燧发手枪（古董）", "Naval Flintlock Pistol (Antique)",
                    "一把海军燧发手枪。短小枪身带着船舱与甲板的年代感，足够给一柜古董添上几分海上风情。",
                    "A naval flintlock pistol with a compact, unmistakably old-fashioned frame. A little shipboard character for a cabinet full of landlocked antiques."),
                ["baker"] = new Entry("贝克线膛步枪（古董）", "Baker Rifle (Antique)",
                    "一支贝克线膛步枪。朴实木托配上早期线膛设计，是英国步枪兵时代留给收藏家的纪念。",
                    "A Baker rifle with plain wooden furniture and an early rifled barrel. A collector's keepsake from the age of British riflemen."),
                ["hall_1819"] = new Entry("霍尔M1819后装步枪（古董）", "Hall M1819 Breechloading Rifle (Antique)",
                    "一支霍尔M1819后装步枪。独特的枪尾结构记录着火器设计的转折，在一排前装古董中格外醒目。",
                    "A Hall M1819 breechloading rifle. Its distinctive breech marks a turning point in firearm design and stands out in any row of muzzleloading antiques."),
                ["dreyse"] = new Entry("德莱塞针发步枪（古董）", "Dreyse Needle Rifle (Antique)",
                    "一支德莱塞针发步枪。修长枪身保留着古典军械的模样，针发与后装设计却曾是那个时代的新风潮。",
                    "A Dreyse needle rifle. Its long frame looks thoroughly traditional, though its needle ignition and breechloading design once represented a new age."),
                ["colt_1851"] = new Entry("柯尔特1851海军型转轮手枪（古董）", "Colt 1851 Navy Revolver (Antique)",
                    "一把柯尔特1851海军型转轮手枪。纤细枪管、木质握把与经典转轮，足以让古董柜台多几分风度。",
                    "A Colt 1851 Navy revolver. A slender barrel, wooden grip and classic cylinder lend a little elegance to the antique counter."),
                ["mississippi_1841"] = new Entry("1841式密西西比击发步枪（古董）", "Model 1841 Mississippi Percussion Rifle (Antique)",
                    "一支1841式密西西比击发步枪。厚实木托与传统军用轮廓，保留着击发枪时代的沉稳气派。",
                    "A Model 1841 Mississippi percussion rifle. Substantial woodwork and a traditional military profile preserve the dignity of the percussion era."),
                ["colt_saa"] = new Entry("柯尔特单动军用转轮手枪", "Colt Single Action Army Revolver",
                    "一把使用.45 Colt弹药的单动转轮手枪。结实枪身与经典轮廓，让遥远的边疆在柜台前近了一步。",
                    "A single-action revolver chambered in .45 Colt. Its sturdy frame and classic lines bring a little of the frontier to the counter."),
                ["m1911"] = new Entry("M1911半自动手枪", "M1911 Semi-Automatic Pistol",
                    "一把使用.45 ACP弹药的半自动手枪。老派、硬朗，总有人愿意为它是不是最好的手枪争上半天。",
                    "A semi-automatic pistol chambered in .45 ACP. Old-fashioned and solidly built; someone is always willing to argue that it is still the best."),
                ["winchester1873"] = new Entry("温彻斯特M1873杠杆步枪", "Winchester Model 1873 Lever-Action Rifle",
                    "一支使用.44-40 WCF弹药的杠杆步枪。木托与标志性的杠杆让它自带西部风情，无须再挂一顶牛仔帽。",
                    "A lever-action rifle chambered in .44-40 WCF. Its wooden stock and distinctive lever provide all the frontier character; a cowboy hat is optional."),
                ["mosin1891"] = new Entry("莫辛-纳甘M1891栓动步枪", "Mosin-Nagant M1891 Bolt-Action Rifle",
                    "一支使用7.62×54R弹药的栓动步枪。枪身修长，线条朴实，满是老式军用步枪不爱拐弯抹角的作风。",
                    "A bolt-action rifle chambered in 7.62×54R. Long, plain and thoroughly military, with little interest in fashionable embellishments."),
                ["kar98k"] = new Entry("毛瑟Kar98k栓动卡宾枪", "Mauser Kar98k Bolt-Action Carbine",
                    "一支使用7.92×57毛瑟弹药的栓动卡宾枪。紧凑木托与利落枪机，保留着传统军用步枪的严整气质。",
                    "A bolt-action carbine chambered in 7.92×57 Mauser. Its compact wooden stock and clean profile retain the disciplined character of a traditional service rifle."),
                ["m1garand"] = new Entry("M1加兰德半自动步枪", "M1 Garand Semi-Automatic Rifle",
                    "一支使用.30-06弹药的半自动步枪。厚实木托与整装弹夹是它的经典招牌，老兵与收藏家都认得这副模样。",
                    "A semi-automatic rifle chambered in .30-06. Its substantial wooden stock and en bloc clip give it a profile familiar to veterans and collectors alike."),
                ["thompson1928"] = new Entry("汤普森M1928A1冲锋枪", "Thompson M1928A1 Submachine Gun",
                    "一支使用.45 ACP弹药的汤普森冲锋枪。厚重金属配上木制握把与枪托，带着老电影般的醒目派头。",
                    "A Thompson submachine gun chambered in .45 ACP. Heavy metal and wooden furniture give it a presence straight out of an old film."),
                ["ppsh41"] = new Entry("波波沙PPSh-41冲锋枪（弹鼓型）", "PPSh-41 Submachine Gun (Drum)",
                    "一支使用7.62×25弹药的冲锋枪。圆鼓鼓的弹鼓与木制枪托格外醒目，看起来像是为漫长战事准备的。",
                    "A submachine gun chambered in 7.62×25. Its round drum and wooden stock are instantly recognizable; it looks prepared for a long war."),
                ["winchester1897"] = new Entry("温彻斯特M1897泵动战壕霰弹枪", "Winchester Model 1897 Pump-Action Trench Shotgun",
                    "一支12号口径泵动霰弹枪。短枪管与老式木托保留着战壕枪的硬派轮廓，绝不是精巧的摆设。",
                    "A 12-gauge pump-action shotgun. Its short barrel and old-fashioned woodwork retain the blunt character of a trench gun, not a delicate display piece."),
                ["bar1918"] = new Entry("勃朗宁M1918自动步枪", "Browning M1918 Automatic Rifle",
                    "一支使用.30-06弹药的自动步枪。宽厚机匣与朴实木托相当有分量，连摆进货架都像一项体力活。",
                    "An automatic rifle chambered in .30-06. Its substantial receiver and plain wooden furniture make even stocking the shelf feel like physical labor."),
                ["mg42"] = new Entry("MG 42通用机枪", "MG 42 General-Purpose Machine Gun",
                    "一挺使用7.92×57毛瑟弹药的通用机枪。长机匣与散热护套组成了极具辨识度的战场轮廓。",
                    "A general-purpose machine gun chambered in 7.92×57 Mauser. Its long receiver and distinctive barrel shroud form an unmistakable battlefield silhouette."),
                ["stg44"] = new Entry("StG 44突击步枪", "StG 44 Assault Rifle",
                    "一支使用7.92×33 Kurz弹药的突击步枪。弧形弹匣与木托之间，藏着军用步枪迈向新时代的身影。",
                    "An assault rifle chambered in 7.92×33 Kurz. Between its curved magazine and wooden stock lies the outline of a new era in service rifles."),
                ["ak47"] = new Entry("AK-47突击步枪", "AK-47 Assault Rifle",
                    "一支使用7.62×39毫米弹药的突击步枪。朴实、硬朗，这玩意就算到了宇宙中也随处可见。",
                    "An assault rifle chambered in 7.62×39 mm. Plain and hard-edged; you can find these things everywhere, even out in space."),
                ["glock17"] = new Entry("格洛克GLOCK 17半自动手枪", "GLOCK 17 Semi-Automatic Pistol",
                    "一把使用9×19毫米弹药的半自动手枪。聚合物枪身与方正套筒不追求华丽，却很懂得实用二字。",
                    "A semi-automatic pistol chambered in 9×19 mm. Its polymer frame and squared slide make no attempt at grandeur, just a convincing case for practicality."),
                ["m16a1"] = new Entry("M16A1突击步枪", "M16A1 Assault Rifle",
                    "一支使用5.56×45毫米弹药的突击步枪。细长枪管与固定提把，勾勒出冷战军械中熟悉的轮廓。",
                    "An assault rifle chambered in 5.56×45 mm. A slender barrel and fixed carry handle give it a familiar Cold War profile."),
                ["fnfal"] = new Entry("FN FAL战斗步枪", "FN FAL Battle Rifle",
                    "一支使用7.62×51毫米弹药的战斗步枪。修长而结实的全尺寸枪身，带着旧式军用长枪的庄重。",
                    "A battle rifle chambered in 7.62×51 mm. Its long, substantial frame carries the imposing character of a full-size service rifle."),
                ["mp5"] = new Entry("HK MP5A2冲锋枪", "HK MP5A2 Submachine Gun",
                    "一支使用9×19毫米弹药的冲锋枪。紧凑枪身配上固定枪托，在一排长枪之间显得格外利落。",
                    "A submachine gun chambered in 9×19 mm. A compact frame and fixed stock make it look particularly tidy beside a rack of full-length rifles."),
                ["p90"] = new Entry("FN P90个人防卫武器", "FN P90 Personal Defense Weapon",
                    "一支使用5.7×28毫米弹药的个人防卫武器。顶部弹匣与圆润枪身颇有未来感，放进太空商店也毫不违和。",
                    "A personal defense weapon chambered in 5.7×28 mm. Its top-mounted magazine and rounded frame look futuristic enough to feel at home in a spaceborne shop."),
                ["sw629"] = new Entry("史密斯威森629型转轮手枪（6英寸）", "Smith & Wesson Model 629 Revolver (6-inch)",
                    "一把使用.44 Magnum弹药的转轮手枪。亮眼金属与修长枪管很有派头，精致的外表并不代表温柔。",
                    "A revolver chambered in .44 Magnum. Bright metal and a long barrel give it plenty of presence; a refined appearance need not imply a gentle disposition."),
                ["rem870"] = new Entry("雷明顿870泵动霰弹枪（短枪管型）", "Remington 870 Pump-Action Shotgun (Short Barrel)",
                    "一支12号口径泵动霰弹枪。结实枪身与短枪管朴实无华，很适合不喜欢花哨货色的买家。",
                    "A 12-gauge pump-action shotgun. Solid construction and a short barrel keep things straightforward, for buyers with little patience for frills."),
                ["m24"] = new Entry("M24栓动狙击步枪", "M24 Bolt-Action Sniper Rifle",
                    "一支使用7.62×51毫米弹药的栓动狙击步枪。沉稳枪身更衬耐心的射手，而不是急着讨价还价的收藏家。",
                    "A bolt-action sniper rifle chambered in 7.62×51 mm. Its restrained profile suits a patient marksman better than an impatient bargain hunter."),
                ["m82a1"] = new Entry("巴雷特M82A1大口径半自动步枪", "Barrett M82A1 Large-Caliber Semi-Automatic Rifle",
                    "一支使用.50 BMG弹药的大口径半自动步枪。尺寸与气势都不容小觑，小店摆放前最好先腾一腾柜台。",
                    "A large-caliber semi-automatic rifle chambered in .50 BMG. Neither its size nor its presence is modest; a small shop should clear some counter space first."),
                ["m249"] = new Entry("M249班用机枪", "M249 Squad Automatic Weapon",
                    "一挺使用5.56×45毫米弹药的班用机枪。厚实机匣与军用轮廓，让它在货架上也像随时等待队伍集合。",
                    "A squad automatic weapon chambered in 5.56×45 mm. Its substantial receiver and military profile look ready for the squad to form up, even on a shop shelf."),
                ["m60"] = new Entry("M60通用机枪", "M60 General-Purpose Machine Gun",
                    "一挺使用7.62×51毫米弹药的通用机枪。粗犷外形与十足分量，让搬货的人比买家更早记住它。",
                    "A general-purpose machine gun chambered in 7.62×51 mm. Its rugged frame and considerable bulk make an impression on the stock clerk before the customer."),
                ["markiv"] = new Entry("鲁格Mark IV标准型靶射手枪", "Ruger Mark IV Standard Target Pistol",
                    "一把小口径靶射手枪。细长枪管与简洁轮廓透着专注，比起张扬外观，它更适合安静的靶场时光。",
                    "A small-caliber target pistol. Its slender barrel and clean lines suggest quiet time at the range rather than a taste for extravagant appearances."),
                ["p22"] = new Entry("瓦尔特P22半自动手枪", "Walther P22 Semi-Automatic Pistol",
                    "一把小口径半自动手枪。轻巧枪身与紧凑轮廓，让摆满大块头的柜台也能为它留出一席之地。",
                    "A small-caliber semi-automatic pistol. Its light, compact frame can find a place on a counter already crowded with larger hardware."),
                ["buckmark"] = new Entry("勃朗宁Buck Mark靶射手枪", "Browning Buck Mark Target Pistol",
                    "一把小口径靶射手枪。木质握把为利落枪身添上几分传统气质，是柜台上一件耐看的小物。",
                    "A small-caliber target pistol. A wooden grip adds a traditional touch to its purposeful frame, making it a pleasing piece for the counter."),
                ["1022"] = new Entry("鲁格10/22半自动步枪", "Ruger 10/22 Semi-Automatic Rifle",
                    "一支小口径半自动步枪。木制枪托与轻巧枪身少了军械的锋芒，多了休闲靶场的轻松气息。",
                    "A small-caliber semi-automatic rifle. Its wooden stock and light frame trade military severity for the relaxed character of a recreational range gun."),
                ["winchester37"] = new Entry("温彻斯特37型单管霰弹枪", "Winchester Model 37 Single-Barrel Shotgun",
                    "一支使用12/70霰弹的单管单发霰弹枪。木托与朴实枪身没有多余装饰，是老派实用主义的代表。",
                    "A single-barrel, single-shot shotgun chambered in 12/70. Plain woodwork and an unadorned frame make a convincing case for old-fashioned practicality."),
                ["fp45"] = new Entry("FP-45解放者单发手枪", "FP-45 Liberator Single-Shot Pistol",
                    "一把使用.45 ACP弹药的单发手枪。外形粗陋得近乎吝啬，却凭着鲜明来历成为收藏柜里惹人好奇的一件。",
                    "A single-shot pistol chambered in .45 ACP. Almost miserly in its construction, yet its unusual heritage makes it a conversation piece in a collector's cabinet."),
                ["texas_defender"] = new Entry("邦德武器得克萨斯卫士双管德林杰手枪", "Bond Arms Texas Defender Double-Barrel Derringer",
                    "一把使用.45 Colt弹药的双管德林杰手枪。短小枪身叠着两根枪管，尺寸不大，派头倒不小。",
                    "A double-barrel derringer chambered in .45 Colt. Two stacked barrels in a very small frame; modest dimensions, decidedly less modest character."),
                ["tt33"] = new Entry("托卡列夫TT-33半自动手枪", "Tokarev TT-33 Semi-Automatic Pistol",
                    "一把使用7.62×25毫米托卡列夫弹药的半自动手枪。纤细而硬朗的外形，保留着旧式军械的简洁作风。",
                    "A semi-automatic pistol chambered in 7.62×25 mm Tokarev. Its slim, hard-edged profile retains the no-nonsense character of an older service arm."),
                ["svt40"] = new Entry("托卡列夫SVT-40半自动步枪", "Tokarev SVT-40 Semi-Automatic Rifle",
                    "一支使用7.62×54R弹药的半自动步枪。修长木托与醒目护木带着旧式军用长枪的气派，摆上架便很占眼缘。",
                    "A semi-automatic rifle chambered in 7.62×54R. A long wooden stock and distinctive handguard give it the presence of a classic service rifle."),
                ["springfield1903"] = new Entry("斯普林菲尔德M1903栓动步枪", "Springfield M1903 Bolt-Action Rifle",
                    "一支使用.30-06 Springfield弹药的栓动步枪。修长木托与传统枪机勾勒出端正轮廓，颇受老派收藏家青睐。",
                    "A bolt-action rifle chambered in .30-06 Springfield. Its long wooden stock and traditional bolt give it the dignified profile favored by old-school collectors."),
                ["beretta92fs"] = new Entry("伯莱塔92FS半自动手枪", "Beretta 92FS Semi-Automatic Pistol",
                    "一把使用9×19毫米弹药的半自动手枪。开放式套筒与圆润线条颇具辨识度，实用之外也有几分意式风度。",
                    "A semi-automatic pistol chambered in 9×19 mm. An open-top slide and rounded lines lend a little Italian elegance to a thoroughly practical sidearm."),
                ["sks"] = new Entry("西蒙诺夫SKS半自动卡宾枪", "Simonov SKS Semi-Automatic Carbine",
                    "一支使用7.62×39毫米弹药的半自动卡宾枪。厚实木托与利落轮廓，让它在老式步枪和现代军械之间自成一格。",
                    "A semi-automatic carbine chambered in 7.62×39 mm. Substantial woodwork and a tidy profile give it a character poised between traditional rifles and modern service arms."),
                ["mini14"] = new Entry("鲁格Mini-14牧场型半自动步枪", "Ruger Mini-14 Ranch Semi-Automatic Rifle",
                    "一支使用5.56×45毫米NATO弹药的半自动步枪。木托与紧凑枪身相映成趣，带着几分牧场工具般的朴实气质。",
                    "A semi-automatic rifle chambered in 5.56×45 mm NATO. A wooden stock and compact frame give it the unpretentious character of a well-kept ranch tool."),
                ["mp40"] = new Entry("MP 40冲锋枪", "MP 40 Submachine Gun",
                    "一支使用9×19毫米弹药的冲锋枪。折叠枪托与冷峻金属线条，使它在一排木托老枪中格外显眼。",
                    "A submachine gun chambered in 9×19 mm. Its folding stock and austere metal lines stand apart from a rack of wood-stocked contemporaries."),
                ["m4a1"] = new Entry("M4A1卡宾枪", "M4A1 Carbine",
                    "一支使用5.56×45毫米NATO弹药的卡宾枪。紧凑枪身与伸缩枪托带着鲜明的现代军械风格，适合偏爱利落造型的买家。",
                    "A carbine chambered in 5.56×45 mm NATO. A compact frame and telescoping stock give it a distinctly modern military look for buyers who favor purposeful lines."),
                ["bfg9000"] = new Entry("毁灭战士BFG 9000能量狙击枪", "DOOM BFG 9000 Energy Sniper Rifle",
                    "来自《毁灭战士》的虚构能量狙击枪。庞大的枪身与科幻轮廓极具压迫感，是足以独占一层货架的跨界收藏品。",
                    "A fictional energy sniper rifle from DOOM. Its massive frame and imposing science-fiction silhouette make it a crossover collectible worthy of a shelf all to itself.")
            });

        public static string Name(string key, bool english)
        {
            var entry = Get(key);
            return english ? entry.EnName : entry.ZhName;
        }
        public static string Description(string key, bool english)
        {
            var entry = Get(key);
            return english ? entry.EnDescription : entry.ZhDescription;
        }
        private static Entry Get(string key)
        {
            if (key != null && Entries.TryGetValue(key, out var entry)) return entry;
            throw new ArgumentException("Missing firearm text key: " + key, nameof(key));
        }
    }
}
