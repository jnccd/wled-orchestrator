import useWledOrchState from "../hooks/useWledOrchState";
import { Box, HStack, Text } from "@chakra-ui/react";
import DraggableButton from "./DraggableButton";
import apiClient from "../services/api-client";
import { AxiosError, CanceledError } from "axios";
import { useState } from "react";

const ledSegmentClassName = "led-segment-group";

interface Props {
  serverButtonIdPrefix: string;
}

const WledSegmentGroupViewer = ({ serverButtonIdPrefix }: Props) => {
  // Get API state
  const {
    wledOrchState,
    hasData: stateLoaded,
    refresh: refreshState,
  } = useWledOrchState();
  // Group selection
  const [selectedGroupId, setSelectedGroupId] = useState("");
  if (
    stateLoaded &&
    wledOrchState.ledSegmentGroups.groups?.filter(
      (x) => x.id === selectedGroupId
    ).length === 0
  )
    setSelectedGroupId(wledOrchState.ledSegmentGroups.groups?.at(0)?.id ?? "");

  // Drag event
  const onDragEnd = (elem: HTMLElement, mousePos: number[]) => {
    const hitGroup = document
      .elementsFromPoint(mousePos[0], mousePos[1])
      .filter((x) => x.classList.contains(ledSegmentClassName));
    const hitGroupData = wledOrchState.ledSegmentGroups?.groups?.filter(
      (x) => x.id === hitGroup[0]?.classList[1]
    );

    apiClient
      .put("/state/moveSegment", null, {
        params: {
          segmentId: elem.classList[1],
          targetGroupId: hitGroupData ? hitGroupData[0]?.id : "",
        },
      })
      .then((_) => {
        refreshState();
      })
      .catch((err: AxiosError) => {
        if (err instanceof CanceledError) return;
        console.log(err);
      });
  };

  return (
    stateLoaded && (
      <HStack justifyContent="center" width="100%">
        {wledOrchState.ledSegmentGroups?.groups?.map((g) => (
          <Box
            key={g.id}
            className={
              ledSegmentClassName + " " + (g.id ?? "error-id-less-group")
            }
            borderWidth={selectedGroupId === g.id ? "3px" : "1px"}
            borderRadius="lg"
            margin={2}
            padding="2px"
            width={"fit-content"}
            transition="border .1s"
            onClick={() => {
              if (g.id) setSelectedGroupId(g.id);
            }}
          >
            <Text>{g.name}</Text>
            {g.ledSegments &&
              g.ledSegments.map((s) => (
                <DraggableButton
                  key={s.readonlyId}
                  className={s.readonlyId ?? "error-id-less-segment"}
                  buttonName={
                    s.wledServerAddress?.split(".").slice(-1)[0] ?? "Not Found"
                  }
                  id={
                    serverButtonIdPrefix +
                    "-" +
                    s.wledServerAddress?.split(".").slice(-1)[0]
                  }
                  onDragEnd={onDragEnd}
                ></DraggableButton>
              ))}
          </Box>
        ))}
      </HStack>
    )
  );
};

export default WledSegmentGroupViewer;
