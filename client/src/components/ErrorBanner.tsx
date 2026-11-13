export function ErrorBanner({ messages }: { messages: string[] }) {
  if (messages.length === 0) return null;
  return (
    <div className="banner banner-error" role="alert">
      {messages.length === 1 ? (
        messages[0]
      ) : (
        <ul>
          {messages.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </ul>
      )}
    </div>
  );
}
