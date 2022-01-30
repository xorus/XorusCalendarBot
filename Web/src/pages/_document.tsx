import Document, {
    Html,
    Head,
    Main,
    NextScript,
    DocumentContext,
} from 'next/document';
import {InitializeColorMode} from 'theme-ui';

class MyDocument extends Document {
    static async getInitialProps(ctx: DocumentContext) {
        const initialProps = await Document.getInitialProps(ctx);
        return {...initialProps};
    }

    render() {
        return (
            <Html>
                <Head>
                    <link rel="preconnect" href="https://fonts.googleapis.com"/>
                    <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin={'*'}/>
                    <link href="https://fonts.googleapis.com/css2?family=Mochiy+Pop+P+One&family=Cabin&display=swap"
                          rel="stylesheet"/>
                </Head>
                <body>
                <InitializeColorMode/>
                <Main/>
                <NextScript/>
                </body>
            </Html>
        );
    }
}

export default MyDocument;
