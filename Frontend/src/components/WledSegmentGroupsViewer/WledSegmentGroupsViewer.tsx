import useWledOrchState from "../../hooks/useWledOrchState";
import { HStack } from "@chakra-ui/react";
import { useState } from "react";
import WledSegmentGroupViewer from "./WledSegmentGroupViewer";

interface Props {}

const WledSegmentGroupsViewer = ({}: Props) => {
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

  return (
    stateLoaded && (
      <HStack justifyContent="center" width="100%">
        {wledOrchState.ledSegmentGroups?.groups?.map((g) => (
          <WledSegmentGroupViewer
            group={g}
            refreshWledGroupViewer={refreshState}
            selectedGroupId={selectedGroupId}
            setSelectedGroupId={setSelectedGroupId}
            wledOrchState={wledOrchState}
            key={g.id}
          ></WledSegmentGroupViewer>
        ))}
      </HStack>
    )
  );
};

export default WledSegmentGroupsViewer;
