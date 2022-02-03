import {useField} from "formik";
import React, {useCallback, useEffect, useRef} from "react";
import {Box, Input, Label, Select} from "theme-ui";

export const TimeOffset = (props: {
    calendarId: string,
    field: string
}) => {
    const [field, meta, helpers] = useField(props.field);

    const hoursRef = useRef<HTMLInputElement>(null);
    const minutesRef = useRef<HTMLInputElement>(null);
    const secondsRef = useRef<HTMLInputElement>(null);
    const signRef = useRef<HTMLSelectElement>(null);

    const reMath = useCallback(() => {
        if (!signRef.current || !hoursRef.current || !minutesRef.current || !secondsRef.current) return;

        let hours = parseInt(hoursRef.current.value);
        let minutes = parseInt(minutesRef.current.value);
        let seconds = parseInt(secondsRef.current.value);

        let newSeconds = seconds;
        let newMinutes = minutes;
        let newHours = hours;

        if (seconds < 0) {
            newSeconds = 59;
            newMinutes -= 1;
        } else if (seconds > 60) {
            newSeconds = 0;
            newMinutes += 1;
        }
        if (minutes < 0) {
            newMinutes = 59;
            newHours -= 1;
        } else if (minutes > 60) {
            newMinutes = 0;
            newHours += 1;
        }

        if (newSeconds !== seconds) secondsRef.current.value = "" + newSeconds;
        if (newMinutes !== minutes) minutesRef.current.value = "" + newMinutes;
        if (newHours !== hours) hoursRef.current.value = "" + newHours;

        helpers.setValue(
            (signRef.current.value === "-" ? -1 : 1) * (
                (parseInt(hoursRef.current.value) * 60 * 60)
                + (parseInt(minutesRef.current.value) * 60)
                + (parseInt(secondsRef.current.value))
            )
        )
    }, [hoursRef, minutesRef, secondsRef, signRef, helpers]);

    useEffect(() => {
        if (!signRef.current || !hoursRef.current || !minutesRef.current || !secondsRef.current) return;
        console.log("reset")
        signRef.current.selectedIndex = parseInt(field.value) <= 0 ? 0 : 1;
        hoursRef.current.value = "" + Math.floor(Math.abs(field.value) / 60 / 60);
        minutesRef.current.value = "" + Math.floor(Math.abs(field.value) / 60 % 60);
        secondsRef.current.value = "" + Math.floor(Math.abs(field.value) % 60);
    }, [field, hoursRef, minutesRef, secondsRef, signRef])

    return <Box variant="layout.formRow" style={{width: "auto"}}>
        <Label htmlFor={`cal${props.calendarId}+ReminderOffsetSeconds_h`}>Notification delay</Label>
        <Box>
            <Input title={"hours"} onChange={e => reMath()}
                   defaultValue={Math.floor(Math.abs(field.value) / 60 / 60)}
                   style={{display: "inline-block", width: "4em"}}
                   type={"number"} min={0} ref={hoursRef}/>
            <Input title={"minutes"} onChange={e => reMath()}
                   defaultValue={Math.floor(Math.abs(field.value) / 60 % 60)}
                   style={{display: "inline-block", width: "4em"}}
                   type={"number"} min={-1} max={61} mx={1} ref={minutesRef}/>
            <Input title={"seconds"} onChange={e => reMath()}
                   defaultValue={Math.floor(Math.abs(field.value) % 60)}
                   style={{display: "inline-block", width: "4em"}}
                   type={"number"} min={-1} max={61} ref={secondsRef}/>
            <Box sx={{
                width: "auto",
                display: "inline-block",
                marginLeft: 1
            }}>
                <Select as={"select"} name={""} ref={signRef} onChange={e => reMath()}>
                    <option value={"-"}>before event</option>
                    <option value={"+"}>after event (why?)</option>
                </Select>
            </Box>
        </Box>
        {/*<Box>*/}
        {/*    /!*<Label htmlFor={`cal${props.calendarId}+ReminderOffsetSeconds_h`}>Hours :</Label>*!/*/}
        {/*</Box>*/}
        {/*<Box>*/}
        {/*    <Label htmlFor={`cal${props.calendarId}+ReminderOffsetSeconds_m`}>Minutes</Label>*/}
        {/*</Box>*/}
    </Box>;
}
