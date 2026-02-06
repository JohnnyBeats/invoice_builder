import { Pagination } from '../../src/components/Pagination';

describe('Pagination', () => {
  it('renders page info and navigation buttons', () => {
    cy.mount(
      <Pagination
        page={1}
        totalPages={5}
        totalCount={50}
        pageSize={10}
        onPageChange={() => {}}
      />
    );

    cy.contains('Showing 1 to 10 of 50 results');
    cy.contains('Page 1 of 5');
  });

  it('disables previous button on first page', () => {
    cy.mount(
      <Pagination
        page={1}
        totalPages={5}
        totalCount={50}
        pageSize={10}
        onPageChange={() => {}}
      />
    );

    cy.get('button').first().should('be.disabled');
    cy.get('button').last().should('not.be.disabled');
  });

  it('disables next button on last page', () => {
    cy.mount(
      <Pagination
        page={5}
        totalPages={5}
        totalCount={50}
        pageSize={10}
        onPageChange={() => {}}
      />
    );

    cy.get('button').first().should('not.be.disabled');
    cy.get('button').last().should('be.disabled');
  });

  it('calls onPageChange with correct page number', () => {
    const onPageChange = cy.stub().as('pageChange');

    cy.mount(
      <Pagination
        page={3}
        totalPages={5}
        totalCount={50}
        pageSize={10}
        onPageChange={onPageChange}
      />
    );

    // Click next
    cy.get('button').last().click();
    cy.get('@pageChange').should('have.been.calledWith', 4);

    // Click previous
    cy.get('button').first().click();
    cy.get('@pageChange').should('have.been.calledWith', 2);
  });

  it('shows correct range on middle page', () => {
    cy.mount(
      <Pagination
        page={3}
        totalPages={5}
        totalCount={47}
        pageSize={10}
        onPageChange={() => {}}
      />
    );

    cy.contains('Showing 21 to 30 of 47 results');
  });

  it('shows correct range on last partial page', () => {
    cy.mount(
      <Pagination
        page={5}
        totalPages={5}
        totalCount={47}
        pageSize={10}
        onPageChange={() => {}}
      />
    );

    cy.contains('Showing 41 to 47 of 47 results');
  });
});
