import Link from 'next/link';
import {useColorMode, Container, Flex, NavLink, Button, Box} from 'theme-ui';
import {useAuth} from "../../lib/auth";
import {apiUrl} from "../../lib/apiUrl";

export default function Header() {
    const [colorMode, setColorMode] = useColorMode();
    const [user, _, logout] = useAuth();

    return (
        // see theme.layout.container for styles
        <Container as="header">
            <Flex as="nav">
                {/* passHref is required with NavLink */}
                {/*<Link href="/" passHref>*/}
                {/*    <NavLink p={2}>Home</NavLink>*/}
                {/*</Link>*/}
                <Link href="/calendars" passHref>
                    <NavLink p={2}>Calendars</NavLink>
                </Link>

                <Box ml="auto">
                    {user ? <Button onClick={logout} type='button' variant='muted' mx={'2'}>Logout</Button>
                        : <Link href={apiUrl('/api/auth/discord/login')} passHref><Button
                            as="a" mx={'2'}>Login</Button></Link>}
                    <Button as='a' variant='muted' type='button'
                            onClick={() => setColorMode(colorMode === 'light' ? 'dark' : 'light')}
                    >
                        {colorMode === 'dark' ? '🌙' : '☀️'}
                    </Button>
                </Box>
            </Flex>
        </Container>
    );
}
