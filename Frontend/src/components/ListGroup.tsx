import { useState } from "react";

interface ListGroupProps {
  items: string[];
  heading?: string;
  onSelectedItem?: (item: string) => void;
}

function ListGroup({
  items = [],
  heading = "",
  onSelectedItem = () => {},
}: ListGroupProps) {
  const [selectedIndex, setSelectedIndex] = useState(-1);

  const listContent =
    items.length === 0 ? (
      <p>No items found</p>
    ) : (
      <ul className="list-group">
        {items.map((item, index) => (
          <li
            className={
              selectedIndex === index
                ? "list-group-item active"
                : "list-group-item"
            }
            key={item}
            onClick={() => {
              setSelectedIndex(index);
              onSelectedItem(items[index]);
            }}
          >
            {item}
          </li>
        ))}
      </ul>
    );

  return (
    <>
      <h1 style={{ marginTop: "50px" }}>{heading}</h1>
      {listContent}
    </>
  );
}

export default ListGroup;
