import React, { useState } from "react";
import SelectInput from "../common/SelectInput";

const AssignWaiterToTableForm = ({ waiters, tables, onSubmit }) => {
  const [assignment, setAssignment] = useState({
    waiterId: "",
    tableNumber: ""
  });

  const handleChange = event => {
    setAssignment({ ...assignment, [event.target.name]: event.target.value });
  };

  const handleSubmit = event => {
    event.preventDefault();
    onSubmit(assignment);
  };

  return (
    <form className="form-group" onSubmit={handleSubmit}>
      <SelectInput
        options={waiters.map(waiter => {
          return {
            value: waiter.id,
            text: waiter.shortName
          };
        })}
        label="Waiters"
        name="waiterId"
        value={assignment.waiterId}
        defaultOption="Select waiter"
        onChange={handleChange}
      />
      <SelectInput
        options={tables.map(table => {
          return {
            value: table.number,
            text: table.number
          };
        })}
        label="Tables"
        name="tableNumber"
        value={assignment.tableNumber}
        defaultOption="Select table"
        onChange={handleChange}
      />
      <input type="submit" className="btn btn-success" value="Assign table" />
    </form>
  );
};

export default AssignWaiterToTableForm;
