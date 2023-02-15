namespace WilburWednesdayEmailFunction;

public class EmailItem
{
    public string Subject { get; internal set; }
    public string HtmlBody { get; internal set; }
    public string TextBody { get; internal set; }


    public EmailItem(WilburWednesdayPost post, string imagePrefix)
    {
        this.Subject = post.Title;
        this.HtmlBody = GenerateHtmlBody(post, imagePrefix);
        this.TextBody = post.Body;
    }

    public string GenerateHtmlBody(WilburWednesdayPost post, string imagePrefix)
    {
        return @$"
<div style='border-radius:16px;margin:4px;max-width:400px;width:400px;overflow:hidden;display:block;color:#c4b89b;background-color:#344739;'>
  <div style='padding: 16px; flex: 1 1'>
    <h1>{post.Title}</h1>
    <p>{post.Body}</p>  
  </div>
  <div style='width: 100%; height: 320px; object-fit: fill; position: relative; overflow: hidden; display: flex;'>
    <img alt='Picture of wilbur' src='{imagePrefix}{post.WilburImage}' style='inset: 0px; box-sizing: border-box; padding: 0px; border: none; margin: auto; display: block; width: 0px; height: 0px; min-width: 100%; max-width: 100%; min-height: 100%; max-height: 100%; object-fit: cover;'/>
  </div>  
</div>";
    }
}
