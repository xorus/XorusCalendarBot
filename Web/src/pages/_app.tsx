import App from 'next/app';
import {Container, ThemeProvider} from 'theme-ui';
import NProgress from 'next-nprogress-emotion';

import Header from '../components/Header';
import theme from '../theme';
import {RecoilRoot} from "recoil";
import {LoginGate} from "../components/LoginGate";

class MyApp extends App {
    render() {
        const {Component, pageProps} = this.props;
        return (
            <ThemeProvider theme={theme}>
                <RecoilRoot>
                    <Header/>
                    <NProgress/>
                    <Container>
                        <LoginGate>
                            <Component {...pageProps} />
                        </LoginGate>
                    </Container>
                </RecoilRoot>
            </ThemeProvider>
        );
    }
}

export default MyApp;
