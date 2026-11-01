using System;
namespace AmmoMachineTest {
 internal sealed class AmmoRecipe {
  internal readonly string Id,Zh,En; internal readonly int Price,Powder,Cases,Projectiles;
  // Balance scenario A: sale 80% of base, about 20% material-cost markup; batch power remains separate.
  internal AmmoRecipe(string id,string zh,string en,int price) { Id=id;Zh=zh;En=en;Price=price; Powder=Math.Max(1,(int)Math.Round(price*20.0/27.0,MidpointRounding.AwayFromZero)); Cases=Powder*3; Projectiles=Powder*2; }
 }
 internal static class AmmoRecipes {
  internal static readonly AmmoRecipe[] All={
   new AmmoRecipe("small_pistol_ammo",".22 普通弹",".22 standard",20),new AmmoRecipe("small_pistol_ammo_p",".22 穿甲弹",".22 armor piercing",26),new AmmoRecipe("small_pistol_ammo_nl",".22 非致命弹",".22 non-lethal",14),
   new AmmoRecipe("heavy_pistol_ammo","10毫米普通弹","10mm standard",30),new AmmoRecipe("heavy_pistol_ammo_p","10毫米穿甲弹","10mm armor piercing",36),new AmmoRecipe("heavy_pistol_ammo_nl","10毫米非致命弹","10mm non-lethal",24),
   new AmmoRecipe("earlyfirearms.paper_cartridge","纸包火药和弹丸","Paper cartridge",90),
   new AmmoRecipe("local.modernwars.ammo_45colt",".45 Colt",".45 Colt",35),new AmmoRecipe("local.modernwars.ammo_45acp",".45 ACP",".45 ACP",35),new AmmoRecipe("local.modernwars.ammo_4440",".44-40 WCF",".44-40 WCF",35),new AmmoRecipe("local.modernwars.ammo_76254r","7.62×54R","7.62x54R",35),new AmmoRecipe("local.modernwars.ammo_79257","7.92×57","7.92x57",35),new AmmoRecipe("local.modernwars.ammo_3006",".30-06",".30-06",35),new AmmoRecipe("local.modernwars.ammo_76225","7.62×25","7.62x25",35),new AmmoRecipe("local.modernwars.ammo_79233","7.92×33","7.92x33",35),
   new AmmoRecipe("ak47prototype.ammo","7.62×39","7.62x39",35),new AmmoRecipe("glock17prototype.ammo","9×19","9x19",30),new AmmoRecipe("coldwarmodern.ammo.55645","5.56×45","5.56x45",45),new AmmoRecipe("coldwarmodern.ammo.76251","7.62×51","7.62x51",55),new AmmoRecipe("coldwarmodern.ammo.5728","5.7×28","5.7x28",50),new AmmoRecipe("coldwarmodern.ammo.44mag",".44 Magnum",".44 Magnum",55),new AmmoRecipe("coldwarmodern.ammo.12g","12/70 霰弹","12/70 shells",35),new AmmoRecipe("coldwarmodern.ammo.50bmg",".50 BMG",".50 BMG",110)
  };
  internal static AmmoRecipe Find(string id) { foreach(var x in All)if(x.Id==id)return x;return null; }
 }
}
