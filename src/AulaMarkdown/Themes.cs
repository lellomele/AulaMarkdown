namespace AulaMarkdown;
public sealed record ReaderTheme(string Name,string Window,string Panel,string Text,string Muted,string Line,string Accent,string Paper,string Stripe,string Heading,string Scheme)
{
    public Dictionary<string,string> Colors => new() { ["paper"]=Paper,["text"]=Text,["line"]=Line,["link"]=Accent,["stripe"]=Stripe,["heading"]=Heading,["scheme"]=Scheme };
    public static readonly ReaderTheme[] All = [
        new("Chiaro","#F3F5F8","#FFFFFF","#26344A","#56657B","#DAE2EC","#2364C4","#FFFFFF","#F3F6FA","#17263C","light"),
        new("Carta","#EDE5D5","#F8F2E6","#40372C","#6E604E","#D9CEB8","#176C72","#FFFAEF","#F2ECDf","#342B20","light"),
        new("Scuro","#171B22","#212630","#E5E9F0","#AFBACB","#3C4554","#83BCFF","#212630","#2A313D","#FFFFFF","dark"),
        new("Blu notte","#0C192B","#12223A","#E0EBF9","#A9BCD6","#2F4665","#6CDAD4","#12223A","#1B3050","#F2F7FF","dark"),
        new("Alto contrasto","#000000","#000000","#FFFFFF","#FFFFFF","#FFFFFF","#FFFF00","#000000","#161616","#FFFFFF","dark") ];
}