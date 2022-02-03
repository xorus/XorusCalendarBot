import {FallbackProps} from "react-error-boundary";
import {Alert, Button} from "theme-ui";

export const ErrorFallback = ({error, resetErrorBoundary}: FallbackProps) => {
    return <Alert role="alert">
        <p>Something went wrong:</p>
        <pre>{error.message}</pre>
        <Button variant={"muted"} onClick={resetErrorBoundary}>Try again</Button>
    </Alert>
}