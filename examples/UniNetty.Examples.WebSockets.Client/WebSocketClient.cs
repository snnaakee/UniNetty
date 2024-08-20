// Copyright (c) Microsoft. All rights reserved.
// Copyright (c) Ikpil Choi ikpil@naver.com All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UniNetty.Buffers;
using UniNetty.Codecs.Http;
using UniNetty.Codecs.Http.WebSockets;
using UniNetty.Codecs.Http.WebSockets.Extensions.Compression;
using UniNetty.Handlers.Tls;
using UniNetty.Transport.Bootstrapping;
using UniNetty.Transport.Channels;
using UniNetty.Transport.Channels.Sockets;

namespace UniNetty.Examples.WebSockets.Client
{
    public class WebSocketClient
    {
        public async Task RunClientAsync(X509Certificate2 cert, IPAddress host, int port, string path)
        {
            var builder = new UriBuilder
            {
                Scheme = null != cert ? "wss" : "ws",
                Host = host.ToString(),
                Port = port
            };

            if (!string.IsNullOrEmpty(path))
            {
                builder.Path = path;
            }

            Uri uri = builder.Uri;

            Console.WriteLine("Transport type : Socket");

            IEventLoopGroup group = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Option(ChannelOption.TcpNodelay, true)
                    .Channel<TcpSocketChannel>();

                // Connect with V13 (RFC 6455 aka HyBi-17). You can change it to V08 or V00.
                // If you change it to V00, ping is not supported and remember to change
                // HttpResponseDecoder to WebSocketHttpResponseDecoder in the pipeline.
                var handler = new WebSocketClientHandler(
                    WebSocketClientHandshakerFactory.NewHandshaker(
                        uri, WebSocketVersion.V13, null, true, new DefaultHttpHeaders()));

                bootstrap.Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    if (cert != null)
                    {
                        var targetHost = cert.GetNameInfo(X509NameType.DnsName, false);
                        pipeline.AddLast("tls", new TlsHandler(stream => new SslStream(stream, true, (sender, certificate, chain, errors) => true), new ClientTlsSettings(targetHost)));
                    }

                    pipeline.AddLast(
                        new HttpClientCodec(),
                        new HttpObjectAggregator(8192),
                        WebSocketClientCompressionHandler.Instance,
                        handler);
                }));

                IChannel ch = await bootstrap.ConnectAsync(new IPEndPoint(host, port));
                await handler.HandshakeCompletion;

                Console.WriteLine("WebSocket handshake completed.\n");
                Console.WriteLine("\t[bye]:Quit \n\t [ping]:Send ping frame\n\t Enter any text and Enter: Send text frame");
                while (true)
                {
                    string msg = Console.ReadLine();
                    if (msg == null)
                    {
                        break;
                    }
                    else if ("bye".Equals(msg.ToLower()))
                    {
                        await ch.WriteAndFlushAsync(new CloseWebSocketFrame());
                        break;
                    }
                    else if ("ping".Equals(msg.ToLower()))
                    {
                        var frame = new PingWebSocketFrame(Unpooled.WrappedBuffer(new byte[] { 8, 1, 8, 1 }));
                        await ch.WriteAndFlushAsync(frame);
                    }
                    else
                    {
                        WebSocketFrame frame = new TextWebSocketFrame(msg);
                        await ch.WriteAndFlushAsync(frame);
                    }
                }

                await ch.CloseAsync();
            }
            finally
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }
    }
}