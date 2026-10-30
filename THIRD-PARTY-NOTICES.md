# Third-party material

`Server/FreeTarcoinsHeader.razor` adapts `Libraries/SPTushonka.Server.Web/Pages/Shop/Components/ShopHeader.razor` from [SP-Tushonka/server-csharp](https://github.com/SP-Tushonka/server-csharp), source revision b36816ac8274f86801506d26a420e24d24b3c3fc in the local fork. Original attribution: Archangel / Single Player Tushonka, based on SPT (https://github.com/sp-tarkov/server-csharp).

The original and adapted header are licensed under Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International; see LICENSE-ShopHeader.txt. Changes add a clickable balance, amount-entry dialog, local credit action and refresh. The compiled server DLL includes this adapted material and must retain those license conditions and attribution. Other original FreeTarcoins source is MIT licensed (LICENSE).

Game and SPT assemblies are build references only and are not distributed. At runtime the header reuses the installed SPT shop CSS and image assets; those assets are not included in this repository or package.
