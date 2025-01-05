import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getWledOrchState,
  setActivated,
  wledOrchStateQueryKey,
} from "../hooks/useWledOrchState";
import { IoPower } from "react-icons/io5";

const WledOrchActivationSwitch = () => {
  // React Query setup
  const queryClient = useQueryClient();
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const setActivatedMutation = useMutation({
    mutationFn: setActivated,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });
  const activated = wledOrchStateQuery.data?.activated;

  return (
    <button
      style={{ border: "none", background: "none", outline: "none" }}
      onClick={(_) => setActivatedMutation.mutate({ newActivated: !activated })}
    >
      {!wledOrchStateQuery.isSuccess && <IoPower size={25} color="gray" />}
      {wledOrchStateQuery.isSuccess && activated && (
        <IoPower size={25} color="green" />
      )}
      {wledOrchStateQuery.isSuccess && !activated && (
        <IoPower size={25} color="red" />
      )}
    </button>
  );
};

export default WledOrchActivationSwitch;
