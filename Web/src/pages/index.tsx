import React, {useEffect} from "react";
import Link from 'next/link';
import {useRouter} from "next/router";

const IndexPage = () => {
    const router = useRouter();

    useEffect(() => {
        router.push({
            pathname: "/calendars"
        })
    }, []);

    return <div>
        Redirecting to <Link href={"/calendar"}>/calendar</Link>
    </div>
}

export default IndexPage;