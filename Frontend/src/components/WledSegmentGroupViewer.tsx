import useWledOrchState from "../hooks/useWledOrchState";
import { Box, HStack, Text } from "@chakra-ui/react";
import DraggableButton from "./DraggableButton";
import apiClient from "../services/api-client";
import { AxiosError, CanceledError } from "axios";

const ledSegmentClassName = "led-segment-group";

interface Props {
  serverButtonIdPrefix: string;
}

const WledSegmentGroupViewer = ({ serverButtonIdPrefix }: Props) => {
  const {
    wledOrchState,
    hasData: stateLoaded,
    refresh: refreshState,
  } = useWledOrchState();

  const onDragEnd = (elem: HTMLElement, mousePos: number[]) => {
    const hitGroup = document
      .elementsFromPoint(mousePos[0], mousePos[1])
      .filter((x) => x.classList.contains(ledSegmentClassName));
    const hitGroupData = wledOrchState.ledSegmentGroups?.groups?.filter(
      (x) => x.id === hitGroup[0]?.classList[1]
    );
    console.log(hitGroupData);

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
            className={
              ledSegmentClassName + " " + (g.id ?? "error-id-less-group")
            }
            borderWidth="1px"
            borderRadius="lg"
            margin={2}
            padding="2px"
            width={"fit-content"}
          >
            <Text>{g.name}</Text>
            {g.ledSegments &&
              g.ledSegments.map((s) => (
                <DraggableButton
                  className={s.readonlyId ?? "id-not-found"}
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
