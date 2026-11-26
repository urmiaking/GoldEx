using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace GoldEx.Server.Application.Services;

public sealed record DemoCaptcha(string Id, string Image);

/// <summary>Local, short-lived, single-use image challenge. No external network dependency.</summary>
public sealed class DemoCaptchaService : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 10000 });
    private readonly object _gate = new();
    private static readonly string[] Digits = ["abcdef", "bc", "abdeg", "abcdg", "bcfg", "acdfg", "acdefg", "abc", "abcdefg", "abcdfg"];
    private static readonly (int X1,int Y1,int X2,int Y2)[] Segments = [(4,2,22,2),(24,4,24,18),(24,22,24,36),(4,38,22,38),(2,22,2,36),(2,4,2,18),(4,20,22,20)];
    private sealed record Challenge(string Session, string Purpose, string Answer);

    public DemoCaptcha Issue(string session, string purpose)
    {
        var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        var answer = string.Concat(Enumerable.Range(0, 6).Select(_ => (char)('0' + RandomNumberGenerator.GetInt32(10))));
        _cache.Set(id, new Challenge(session, purpose, answer), new MemoryCacheEntryOptions { Size = 1, AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3) });
        return new(id, Render(answer));
    }

    public bool Verify(string? id, string? answer, string session, string purpose)
    {
        if (id is null || id.Length != 48) return false;
        Challenge? challenge;
        lock (_gate)
        {
            if (!_cache.TryGetValue(id, out challenge) || challenge is null || challenge.Session != session || challenge.Purpose != purpose)
                return false;
            _cache.Remove(id);
        }
        if (answer is null || answer.Length > 16) return false;
        var normalized = string.Concat(answer.Trim().Select(c => c is >= '۰' and <= '۹' ? (char)('0' + c - '۰') : c is >= '٠' and <= '٩' ? (char)('0' + c - '٠') : c));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(normalized), Encoding.UTF8.GetBytes(challenge.Answer));
    }

    private static string Render(string answer)
    {
        const int w = 240, h = 68;
        var pixels = new byte[w*h];
        Array.Fill(pixels, (byte)244);
        void Dot(int x, int y, int r, byte shade)
        {
            for (int dy=-r;dy<=r;dy++) for(int dx=-r;dx<=r;dx++)
                if(dx*dx+dy*dy<=r*r && x+dx>=0 && x+dx<w && y+dy>=0 && y+dy<h)
                    pixels[(y+dy)*w+x+dx]=shade;
        }
        void Line(double x1,double y1,double x2,double y2,int radius,byte shade)
        {
            var steps = Math.Max(1,(int)Math.Ceiling(Math.Max(Math.Abs(x2-x1),Math.Abs(y2-y1))));
            for(var i=0;i<=steps;i++) Dot((int)Math.Round(x1+(x2-x1)*i/steps),(int)Math.Round(y1+(y2-y1)*i/steps),radius,shade);
        }
        for(var i=0;i<180;i++) Dot(RandomNumberGenerator.GetInt32(w),RandomNumberGenerator.GetInt32(h),RandomNumberGenerator.GetInt32(1,3),(byte)RandomNumberGenerator.GetInt32(160,225));
        for(var i=0;i<answer.Length;i++)
        {
            var x=12+i*37; var y=RandomNumberGenerator.GetInt32(10,20);
            var angle=RandomNumberGenerator.GetInt32(-16,17)*Math.PI/180;
            var cos=Math.Cos(angle); var sin=Math.Sin(angle);
            (double X,double Y) Transform(int px,int py) => (x+13+(px-13)*cos-(py-20)*sin,y+20+(px-13)*sin+(py-20)*cos);
            foreach(var c in Digits[answer[i]-'0'])
            {
                var seg=Segments[c-'a']; var a=Transform(seg.X1,seg.Y1); var b=Transform(seg.X2,seg.Y2);
                Line(a.X,a.Y,b.X,b.Y,2,(byte)RandomNumberGenerator.GetInt32(25,85));
            }
        }
        for(var i=0;i<4;i++) Line(0,RandomNumberGenerator.GetInt32(8,60),w-1,RandomNumberGenerator.GetInt32(8,60),0,(byte)RandomNumberGenerator.GetInt32(100,160));
        using var png = new MemoryStream();
        png.Write(new byte[]{137,80,78,71,13,10,26,10});
        var header=new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0,4),w);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4,4),h);
        header[8]=8; header[9]=0;
        Chunk(png,"IHDR",header);
        using var compressed=new MemoryStream();
        using(var z=new ZLibStream(compressed,CompressionLevel.Fastest,true))
            for(int y=0;y<h;y++){ z.WriteByte(0); z.Write(pixels,y*w,w); }
        Chunk(png,"IDAT",compressed.ToArray());
        Chunk(png,"IEND",[]);
        return "data:image/png;base64,"+Convert.ToBase64String(png.ToArray());
    }
    private static void Chunk(Stream output,string name,byte[] data)
    {
        Span<byte> n=stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(n,data.Length); output.Write(n);
        var type=Encoding.ASCII.GetBytes(name); output.Write(type); output.Write(data);
        uint crc=0xffffffff;
        foreach(var b in type.Concat(data))
        {
            crc^=b;
            for(var i=0;i<8;i++) crc=(crc>>1)^((crc&1)!=0?0xedb88320u:0u);
        }
        BinaryPrimitives.WriteUInt32BigEndian(n,~crc); output.Write(n);
    }

    public void Dispose() => _cache.Dispose();
}
