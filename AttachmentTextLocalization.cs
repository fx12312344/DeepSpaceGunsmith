using System;
using System.Collections.Generic;

namespace Attachments010
{
    // Spec.Name remains an audit label. Player-facing text is selected at lookup time.
    public static class AttachmentTextLocalization
    {
        private sealed class Entry
        {
            public readonly string NameZh, NameEn, DescriptionZh, DescriptionEn;
            public Entry(string nameZh, string nameEn, string descriptionZh, string descriptionEn)
            {
                NameZh = nameZh; NameEn = nameEn;
                DescriptionZh = descriptionZh; DescriptionEn = descriptionEn;
            }
        }

        // Exact catalog contract: no inferred prefixes, aliases, or fallback entries.
        private static readonly string[] ExpectedKeyList = {
            "red_dot", "holographic", "scope_1_4", "scope_4_32", "ballistic_sight",
            "light_stock", "telescopic_stock", "heavy_stock", "precision_stock",
            "light_grip", "tactical_grip", "angled_grip", "heavy_grip",
            "grenade_launcher", "underbarrel_shotgun", "tactical_light", "laser",
            "muzzle_brake", "advanced_brake", "flash_hider", "tactical_flash_hider",
            "makeshift_suppressor", "advanced_suppressor", "full_choke", "half_choke",
            "optimized_trigger", "advanced_trigger", "competition_trigger",
            "cleaning", "silver_plating", "gold_plating", "alofs", "m1911_stock",
            "glock_switch", "pedersen", "sks_bump_stock"
        };

        private static readonly Dictionary<string, Entry> Texts = new Dictionary<string, Entry>(StringComparer.Ordinal) {
            ["red_dot"] = new Entry("红点瞄具", "Red Dot Sight",
                "小巧的红点瞄具，给适配枪械添上一点利落的现代气息。提升精度与卖相，不拖累操持。",
                "A compact red dot sight gives compatible firearms a crisp, modern look. It improves accuracy and style without a handling penalty."),
            ["holographic"] = new Entry("全息瞄具", "Holographic Sight",
                "方正的全息瞄具很有战术派头，精度也比基础红点更出色。额外的分量让适配枪械稍显笨拙。",
                "This boxy holographic sight brings tactical flair and more accuracy than a basic red dot. The extra bulk makes compatible firearms a little less handy."),
            ["scope_1_4"] = new Entry("1–4 倍可变倍率瞄准镜", "1–4x Variable Scope",
                "一支外形紧凑的低倍变焦镜，为适配枪械增加精度与几分专业气质。镜身虽短，仍要占去一些操持余裕。",
                "A compact low-power variable scope adds accuracy and a professional air to compatible firearms. Even a short optic takes a little away from handling."),
            ["scope_4_32"] = new Entry("4–32 倍可变倍率瞄准镜", "4–32x Variable Scope",
                "长长的高倍镜让适配枪械有了精密射击的架势，精度提升也相当可观。沉重镜身换来的代价是更费力的操持。",
                "This long high-power scope gives compatible firearms a precision-rifle silhouette and a substantial accuracy boost. Its weight demands a larger sacrifice in handling."),
            ["ballistic_sight"] = new Entry("弹道计算瞄准具", "Ballistic Computing Sight",
                "精密镜体与电子组件挤在一起，科技感和精度同样抢眼。适配枪械的操持负担也随之加重，体面从来不轻巧。",
                "Precision optics and electronics share a crowded housing, bringing impressive accuracy and high-tech style. Compatible firearms become harder to handle; sophistication is not lightweight."),
            ["light_stock"] = new Entry("轻型合成枪托", "Lightweight Synthetic Stock",
                "长枪用合成枪托，朴素外壳省去了不少累赘。操持更轻快，后坐控制也略有改善。",
                "A plain synthetic stock for long guns that sheds unnecessary bulk. It improves handling and offers a modest gain in recoil control."),
            ["telescopic_stock"] = new Entry("战术伸缩枪托", "Tactical Telescopic Stock",
                "棱角分明的长枪枪托，兼顾战术卖相、操持和后坐控制。没有夸张的专长，却也不必为某一项牺牲另一项。",
                "An angular long-gun stock that balances tactical style, handling, and recoil control. It has no extravagant specialty and asks for no trade-off between them."),
            ["heavy_stock"] = new Entry("重型稳定枪托", "Heavy Stabilizing Stock",
                "厚实的长枪枪托，把分量花在后坐控制上。稳定得很有底气，操持起来也确实沉重。",
                "A thick long-gun stock that puts its weight into recoil control. Reassuringly steady, and every bit as cumbersome as it looks."),
            ["precision_stock"] = new Entry("精密射击枪托", "Precision Stock",
                "轮廓讲究的长枪枪托，为精度与后坐控制留足了余地。它更中意沉稳的枪身，而不是轻巧的操持。",
                "A carefully shaped long-gun stock that favors accuracy and recoil control. It prefers a steady weapon to effortless handling."),
            ["light_grip"] = new Entry("轻型垂直握把", "Lightweight Vertical Grip",
                "一只干净利落的长枪下挂握把，把心思全用在改善操持上。没有花哨外壳，也不抢其他配件的风头。",
                "A neat underbarrel grip for long guns, devoted entirely to better handling. No showy housing, and no attempt to steal the rest of the gun's thunder."),
            ["tactical_grip"] = new Entry("战术垂直握把", "Tactical Vertical Grip",
                "棱线鲜明的长枪握把，让外观、后坐控制和操持一起受益。战术味够浓，代价倒不苛刻。",
                "A sharply contoured long-gun grip that improves style, recoil control, and handling together. Plenty of tactical character without an awkward trade-off."),
            ["angled_grip"] = new Entry("斜角握把", "Angled Foregrip",
                "低矮的斜角轮廓贴在长枪下方，着重改善操持。精度也沾上一点光，幅度虽小，总比没有强。",
                "A low-profile angled grip beneath a long gun, focused on easier handling. Accuracy gets a small bonus too; a little is better than nothing."),
            ["heavy_grip"] = new Entry("重型稳定握把", "Heavy Stabilizing Grip",
                "结实得近乎固执的长枪握把，明显加强后坐控制。额外的厚重感也让整枪不那么灵便。",
                "An almost stubbornly solid long-gun grip that substantially improves recoil control. Its extra heft makes the weapon less nimble."),
            ["grenade_launcher"] = new Entry("通用下挂榴弹发射器", "Underbarrel Grenade Launcher",
                "长枪下方的庞然大物，火力、卖相和后坐控制都颇有分量。操持上的沉重负担同样醒目，枪身很难再显得轻盈。",
                "A hulking addition beneath a compatible long gun, with substantial gains in firepower, style, and recoil control. The severe handling penalty is just as conspicuous."),
            ["underbarrel_shotgun"] = new Entry("通用下挂霰弹枪", "Underbarrel Shotgun",
                "紧凑的霰弹枪挂在适配长枪下方，为整枪增添火力和粗犷卖相。多出来的块头让操持明显吃力。",
                "A compact shotgun beneath a compatible long gun adds firepower and rugged style. All that extra bulk takes a clear toll on handling."),
            ["tactical_light"] = new Entry("战术灯", "Tactical Light",
                "紧凑的灯体挂在长枪下方，添上几分整备齐全的气派。卖相与操持都有小幅提升，朴素却讨喜。",
                "A compact light housing gives a long gun a fully outfitted look. Modest improvements to style and handling make it an unassuming favorite."),
            ["laser"] = new Entry("激光指示器", "Laser Module",
                "小巧的激光模块让长枪看起来更干练，操持也更顺手。它的本钱在卖相与手感，而不是精度。",
                "A compact laser module gives a long gun a sharper look and easier handling. Its strengths are style and feel, rather than accuracy."),
            ["muzzle_brake"] = new Entry("标准制退器", "Standard Muzzle Brake",
                "朴实的枪口附件，为适配枪械带来可靠的后坐控制提升和一点硬朗卖相。操持略有损失，算不上沉重负担。",
                "A plain muzzle fitting that improves recoil control and adds a little rugged style to compatible firearms. The handling cost is present, but modest."),
            ["advanced_brake"] = new Entry("高效制退器", "High-Efficiency Muzzle Brake",
                "比标准款更有存在感的制退器，后坐控制与卖相也更出色。适配枪械要为这份沉稳付出更多操持上的代价。",
                "A more prominent brake with stronger recoil control and a sharper look than the standard model. Compatible firearms pay a larger handling cost for that composure."),
            ["flash_hider"] = new Entry("标准消焰器", "Standard Flash Hider",
                "线条简练的枪口附件，给适配枪械收出一个利落的轮廓。略增卖相与操持，不靠夸张性能抢眼。",
                "A clean-lined muzzle fitting that neatly finishes a compatible firearm's profile. Small gains in style and handling are its quiet selling points."),
            ["tactical_flash_hider"] = new Entry("战术消焰器", "Tactical Flash Hider",
                "棱角更鲜明的消焰器，让适配枪械的枪口多些战术气息。卖相与操持的提升都比标准款更足。",
                "A more angular flash hider that lends a compatible firearm some tactical character. Both its style and handling bonuses exceed the standard model's."),
            ["makeshift_suppressor"] = new Entry("简易消音器", "Makeshift Suppressor",
                "做工粗糙的消音附件，连卖相都透着凑合劲。适配枪械的后坐控制略有改善，可靠性与操持却要一起让步。",
                "A rough suppressor whose appearance makes no secret of its improvised origins. Compatible firearms gain a little recoil control at the expense of style, reliability, and handling."),
            ["advanced_suppressor"] = new Entry("高级消音器", "Advanced Suppressor",
                "规整的消音器外壳，为适配枪械添上低调而昂贵的气质。卖相与后坐控制都得到改善，操持则稍受拖累。",
                "A neatly finished suppressor gives compatible firearms an understated, expensive look. It improves style and recoil control with a small handling penalty."),
            ["full_choke"] = new Entry("全喉缩", "Full Choke",
                "霰弹枪专用的全喉缩，偏爱紧凑而集中的表现。精度提升比半喉缩更大，操持则略逊一筹。",
                "A shotgun-only full choke with a taste for tighter performance. It offers more accuracy than the half choke, with a small handling penalty."),
            ["half_choke"] = new Entry("半喉缩", "Half Choke",
                "霰弹枪专用的半喉缩，给精度添一份适中的补益。没有全喉缩那样突出，却也不牺牲操持。",
                "A shotgun-only half choke that provides a moderate accuracy boost. Less pronounced than a full choke, but with no loss of handling."),
            ["optimized_trigger"] = new Entry("优化扳机组", "Refined Trigger Group",
                "细节收拾得更妥帖的扳机组件，为适配枪械补上可靠性与操持。外表不张扬，功夫都在基本功里。",
                "A tidier trigger assembly that improves reliability and handling on compatible firearms. Unassuming on the outside, attentive to the fundamentals."),
            ["advanced_trigger"] = new Entry("高级扳机组", "Advanced Trigger Group",
                "一套做工讲究的扳机组件，在可靠性与操持上比基础优化款更进一步。适配枪械不必靠花哨外观证明身价。",
                "A finely finished trigger assembly that goes beyond the refined model in both reliability and handling. Compatible firearms need no flashy exterior to justify the upgrade."),
            ["competition_trigger"] = new Entry("竞赛扳机组", "Competition Trigger Group",
                "竞赛取向的扳机组件，为适配枪械争取更好的操持与精度。可靠性略有退让，挑剔也是它性格的一部分。",
                "A competition-minded trigger assembly that favors handling and accuracy on compatible firearms. Reliability slips a little; a demanding temperament comes with the package."),
            ["cleaning"] = new Entry("枪械清洁剂", "Firearm Cleaner",
                "一套让适配枪械重拾整洁卖相的清洁用品，老旧外表也能精神几分。光鲜不等于修复，可靠性不会因此提高。",
                "Cleaning supplies that restore a tidy appearance to compatible firearms, even those looking their age. A good shine is not a repair, and reliability stays unchanged."),
            ["silver_plating"] = new Entry("镀银套件", "Silver-Plating Kit",
                "给适配枪械披上一层冷亮的银色，让朴素轮廓也有几分贵气。提升的是卖相，不是火力。",
                "A cool silver finish lends even a plain compatible firearm a touch of refinement. The improvement is in style, not firepower."),
            ["gold_plating"] = new Entry("镀金套件", "Gold-Plating Kit",
                "张扬的金色外衣，专为不愿淹没在货架里的适配枪械准备。卖相比镀银更抢眼，火力却不会被金光抬高。",
                "An ostentatious gold finish for compatible firearms that refuse to disappear on a shelf. More style than silver plating, without a single extra point of firepower."),
            ["alofs"] = new Entry("Alofs 供弹装置", "Alofs Loading Device",
                "Winchester 37 专用的古怪附件，灵感来自历史上的 Alofs 自动装填构想。它以可靠性与操持换取火力，也带着违禁品的麻烦。",
                "An eccentric Winchester 37 exclusive inspired by the historical Alofs loading concept. It trades reliability and handling for firepower, along with the trouble of contraband status."),
            ["m1911_stock"] = new Entry("M1911 可拆卸枪托", "M1911 Detachable Stock",
                "M1911 专用枪托，让这把老派手枪显出不同寻常的轮廓。后坐控制与精度有所提升，操持受限，违禁身份也不容忽视。",
                "An M1911-exclusive stock that gives the old service pistol an unusual silhouette. It improves recoil control and accuracy at a handling cost, with contraband status to consider."),
            ["glock_switch"] = new Entry("GLOCK 17 转换装置", "GLOCK 17 Conversion Device",
                "GLOCK 17 专用的违禁转换附件，小体积藏着极端的取舍。火力大幅提升，可靠性下降，后坐控制更是代价惨重。",
                "A contraband conversion attachment exclusive to the GLOCK 17, with extreme compromises in a small package. Firepower rises sharply, while reliability falls and recoil control suffers severely."),
            ["pedersen"] = new Entry("佩德森转换装置", "Pedersen Device",
                "Springfield M1903 专用的历史奇物，名字来自一战时期的佩德森装置。火力的增长要用可靠性与精度交换，且属于违禁配件。",
                "A historical curiosity exclusive to the Springfield M1903, named after the First World War Pedersen device. This contraband attachment trades reliability and accuracy for firepower."),
            ["sks_bump_stock"] = new Entry("SKS 撞火枪托", "SKS Bump Stock",
                "SKS 专用的违禁枪托，性格比外形更激进。火力有所增长，可靠性、后坐控制和精度却都得付账。",
                "A contraband stock exclusive to the SKS, with a temperament more aggressive than its silhouette. Firepower improves, but reliability, recoil control, and accuracy all pay the bill.")
        };

        static AttachmentTextLocalization()
        {
            ValidateCoverage(ExpectedKeyList);
        }

        public static IReadOnlyList<string> ExpectedKeys => Array.AsReadOnly(ExpectedKeyList);

        public static string Name(string key)
        {
            var text = Texts[key];
            return EnglishLocalization.L.Pick(text.NameZh, text.NameEn);
        }

        public static string Description(string key)
        {
            var text = Texts[key];
            return EnglishLocalization.L.Pick(text.DescriptionZh, text.DescriptionEn);
        }

        // Source/host tests can pass the keys extracted from Catalog.Specs without Unity factories.
        // Fails closed on duplicate, missing, extra, blank, or untranslated entries.
        public static void ValidateCoverage(IEnumerable<string> catalogKeys)
        {
            if (catalogKeys == null) throw new ArgumentNullException(nameof(catalogKeys));
            var expected = new HashSet<string>(ExpectedKeyList, StringComparer.Ordinal);
            if (ExpectedKeyList.Length != 36 || expected.Count != 36 || Texts.Count != 36 || !expected.SetEquals(Texts.Keys))
                throw new InvalidOperationException("Attachment text must cover exactly the 36 declared keys.");
            var actual = new HashSet<string>(StringComparer.Ordinal);
            foreach (string key in catalogKeys)
                if (key == null || !actual.Add(key))
                    throw new InvalidOperationException("Attachment catalog contains a null or duplicate key.");
            if (!expected.SetEquals(actual))
                throw new InvalidOperationException("Attachment text keys and catalog keys differ.");
            var namesZh = new HashSet<string>(StringComparer.Ordinal);
            var namesEn = new HashSet<string>(StringComparer.Ordinal);
            var descriptionsZh = new HashSet<string>(StringComparer.Ordinal);
            var descriptionsEn = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pair in Texts)
            {
                Entry text = pair.Value;
                if (text == null || string.IsNullOrWhiteSpace(text.NameZh) || string.IsNullOrWhiteSpace(text.NameEn)
                    || string.IsNullOrWhiteSpace(text.DescriptionZh) || string.IsNullOrWhiteSpace(text.DescriptionEn)
                    || text.NameZh == text.NameEn || text.DescriptionZh == text.DescriptionEn
                    || !namesZh.Add(text.NameZh) || !namesEn.Add(text.NameEn)
                    || !descriptionsZh.Add(text.DescriptionZh) || !descriptionsEn.Add(text.DescriptionEn))
                    throw new InvalidOperationException("Attachment text is blank, duplicated, or untranslated: " + pair.Key);
            }
        }
    }
}
