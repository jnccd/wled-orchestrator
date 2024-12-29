import {
  getWledOrchState,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchState";
import { HStack } from "@chakra-ui/react";
import { useState } from "react";
import WledSegmentGroupViewer from "./WledSegmentGroupViewer";
import { useQuery } from "@tanstack/react-query";

interface Props {}

const WledSegmentGroupsViewer = ({}: Props) => {
  const query = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });

  // Group selection
  const [selectedGroupId, setSelectedGroupId] = useState("");
  if (
    !query.isPending &&
    query.data?.groups?.filter((x) => x.id === selectedGroupId).length === 0
  )
    setSelectedGroupId(query.data?.groups?.at(0)?.id ?? "");

  return (
    !query.isPending && (
      <HStack justifyContent="center" width="100%">
        {query.data?.groups?.map((g) => (
          <WledSegmentGroupViewer
            group={g}
            selectedGroupId={selectedGroupId}
            setSelectedGroupId={setSelectedGroupId}
            key={g.id}
          ></WledSegmentGroupViewer>
        ))}
      </HStack>
    )
  );
};

export default WledSegmentGroupsViewer;
