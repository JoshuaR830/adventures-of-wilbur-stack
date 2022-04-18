namespace WilburWednesdayEmailFunction;

public class EmailItem
{
    public string Subject { get; internal set; }
    public string HtmlBody { get; internal set; }
    public string TextBody { get; internal set; }

    public EmailItem(WilburWednesdayPost post)
    {
        this.Subject = post.Title;
        this.HtmlBody = post.Body;
        this.TextBody = post.Body;
    }
}
