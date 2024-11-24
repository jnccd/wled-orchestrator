import useWledOrchState from "../hooks/useWledOrchState";
import { Box, Text } from "@chakra-ui/react";
import DraggableButton from "./DraggableButton";

interface Props {
  serverButtonIdPrefix: string;
}

const WledSegmentGroupViewer = ({ serverButtonIdPrefix }: Props) => {
  const { wledOchState, hasData: stateLoaded } = useWledOrchState();

  return (
    stateLoaded && (
      <Box justifyItems="center" width="100%">
        {wledOchState.ledSegmentGroups?.groups?.map((g) => (
          <Box borderWidth="1px" borderRadius="lg" width={"fit-content"}>
            <Text>{g.name}</Text>
            {g.ledSegments &&
              g.ledSegments?.map((s) => {
                return (
                  <DraggableButton
                    buttonName={
                      s.wledServerAddress?.split(".").slice(-1)[0] ??
                      "Not Found"
                    }
                    id={
                      serverButtonIdPrefix +
                      "-" +
                      s.wledServerAddress?.split(".").slice(-1)[0]
                    }
                  ></DraggableButton>
                );
              })}
          </Box>
        ))}
      </Box>
    )
  );
};

export default WledSegmentGroupViewer;
